using Cysharp.Threading.Tasks;
using Project.Core.Ui.GlobalUi;
using Project.Rhythm;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Event;
using Project.Rhythm.Judgement;
using Project.Rhythm.Note;
using Project.Rhythm.Presentation;
using Project.Rhythm.Timeline;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Project.Core.Managers
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private StageData testStageData;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private StagePresenter presenter;

        [SerializeField] private float noteAppearDuration = 2.0f;
        [SerializeField] private float stageStartDelay = 1.5f;

        private AudioTimeline _audioTimeline;
        private RhythmEventSystem _eventSystem;
        private JudgementSystem _judgementSystem;
        private NoteSpawnSystem _noteSpawner;
        private RhythmInputController _inputController;

        private bool _isInitialized;
        private StageData _activeStageData;

        public event Action OnStageStart;
        public event Action OnStageComplete;

        private readonly List<Note> _activeNotes = new();
        public static float CurrentTime { get; private set; }

        private List<(float time, StageThemeType theme)> _themeQueue = new();
        private int _themeIndex = 0;
        private bool _isThemeChanging = false;

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            InitializeSystems(_activeStageData);

            presenter.SetJudgementSystem(_judgementSystem);
            presenter.Initialize(_activeStageData);

            BuildThemeQueue(_activeStageData);

            _noteSpawner = new NoteSpawnSystem(presenter);
            _inputController = new RhythmInputController(_judgementSystem, () => CurrentTime);
            _inputController.OnInputTriggered += InputTriggered;

            BindSystems();
            _isInitialized = true;

            await UniTask.CompletedTask;
        }

        public void Play()
        {
            if (!_isInitialized) return;
            StartSequence(_activeStageData, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void InitializeSystems(StageData stageData)
        {
            _audioTimeline = new AudioTimeline();
            _audioTimeline.Initialize(musicSource, stageData);

            _eventSystem = new RhythmEventSystem();
            _eventSystem.Initialize(stageData, noteAppearDuration);

            _judgementSystem = new JudgementSystem();
            _judgementSystem.Initialize(stageData);
        }

        private void BindSystems()
        {
            _eventSystem.OnCountdownTriggered += (targetBeat) => presenter.StartCountdown(targetBeat);

            _judgementSystem.OnJudged += (result, note) =>
            {
                presenter.GetTouchVisual()?.PlayAction(result);
                note?.OnJudged(result);
            };

            _eventSystem.OnSpawnTriggered += (action, hitTime, duration) =>
            {
                switch (action.noteType)
                {
                    case NoteType.Signal:
                        HandleSignal(action);
                        return;

                    case NoteType.Persistent:
                        HandlePersistent(action, hitTime, duration);
                        return;

                    case NoteType.Runtime:
                        HandleRuntime(action, duration);
                        return;
                }
            };
        }

        private void HandleRuntime(RhythmAction action, float duration)
        {
            var noteObj = _noteSpawner.GetOrSpawn(action, CurrentTime, duration);
            var note = noteObj?.GetComponent<Note>();

            if (note == null) return;

            _judgementSystem.RegisterNote(action, note);

            if (!_activeNotes.Contains(note))
                _activeNotes.Add(note);
        }

        private void HandlePersistent(RhythmAction action, float hitTime, float duration)
        {
            Note note = presenter.GetOrSpawnPersistent(action.targetID);

            if (note == null)
            {
                Debug.LogError($"Persistent Note 생성/조회 실패: {action.targetID}");
                return;
            }
            note.gameObject.SetActive(true);

            note.ResetJudgedState();
            note.InitializePersistent(CurrentTime, duration);

            _judgementSystem.RegisterNote(action, note);

            if (!_activeNotes.Contains(note))
                _activeNotes.Add(note);
        }

        private void HandleSignal(RhythmAction action)
        {
            var note = presenter.GetFixedNote(action.targetID);
            note?.PlaySignal();
        }

        private void Update()
        {
            if (!_isInitialized || _isThemeChanging) return;

            CurrentTime = _audioTimeline.GetStageTime();
            if (CurrentTime < 0f) return;

            presenter.UpdateUI(CurrentTime);
            ProcessThemeChange();
            _eventSystem.Process(CurrentTime);

            _judgementSystem.UpdateHoldCheck(InputManager.Instance.IsPressing, CurrentTime);
            _judgementSystem.CheckMiss(CurrentTime);

            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                if (_activeNotes[i] == null) { _activeNotes.RemoveAt(i); continue; }
                _activeNotes[i].UpdateNote(CurrentTime);
            }

            if (_judgementSystem.IsHolding)
            {
                float progress = _judgementSystem.GetHoldProgress(CurrentTime);
                presenter.GetTouchVisual()?.UpdateVisual(progress);
            }
        }

        private void InputTriggered(PatternType type)
        {
            var visual = presenter.GetTouchVisual();
            if (type == PatternType.None) visual?.StopHoldAction();
            else visual?.PlayAction(type);
        }

        private async UniTask StartSequence(StageData data, CancellationToken token)
        {
            try
            {
                await UniTask.Yield();
                await UniTask.Delay((int)(stageStartDelay * 1000), cancellationToken: token);

                OnStageStart?.Invoke();
                _audioTimeline.StartTimeline();
                await UniTask.WaitUntil(() => _audioTimeline.GetStageTime() >= data.endPosition, cancellationToken: token);

                float finalScore = _judgementSystem.CalculateFinalScore();

                // 1. 점수 저장 전, 최초 클리어 여부를 미리 계산합니다.
                bool isFirstClear = false;
                if (PlayerManager.Instance != null && PlayerManager.Instance.Data != null)
                {
                    var record = PlayerManager.Instance.Data.stageRecords.Find(r => r.stageIndex == data.stageIndex);
                    // 기록이 없거나 베스트 점수가 0이면 최초 클리어입니다.
                    if (record == null || record.bestScore <= 70000) isFirstClear = true;
                }

                // 2. 점수 계산 및 저장은 JudgementSystem이 담당 (데이터 무결성)
                _judgementSystem.FinalizeAndSaveResult();

                // 3. 스테이지를 끝까지 완주했으므로 'true'를 전달하여 플레이 기록을 확정함
                _activeStageData.SetPlayComplete(true);

                // 4. 데이터 수집 및 업적 체크
                int p = _judgementSystem.GetCount(JudgeResult.Perfect);

                // 5. 업적 매니저 호출 (미리 계산한 isFirstClear로 전달합니다)
                if (AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.CheckStageAchievements(data, p, finalScore, isFirstClear);
                }
                // -----------------------------

                presenter.ShowResult(p, _judgementSystem.GetCount(JudgeResult.Great),
                                     _judgementSystem.GetCount(JudgeResult.Good),
                                     _judgementSystem.GetCount(JudgeResult.Miss));

                OnStageComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
        }
        private void BuildThemeQueue(StageData data)
        {
            _themeQueue.Clear();
            _themeIndex = 0;
            if (data.themeEvents == null) return;
            foreach (var evt in data.themeEvents)
            {
                float time = evt.beat * (60f / data.bpm);
                _themeQueue.Add((time, evt.theme));
            }
        }
        private void ProcessThemeChange()
        {
            if (_themeIndex >= _themeQueue.Count) return;
            var next = _themeQueue[_themeIndex];
            if (CurrentTime >= next.time - 0.1f)
            {
                ChangeThemeWithFade(next.theme).Forget();
                _themeIndex++;
            }
        }
        private async UniTaskVoid ChangeThemeWithFade(StageThemeType theme)
        {
            if (_isThemeChanging) return;
            _isThemeChanging = true;

            InputManager.Instance.SetBlockInput(true);
            await GlobalUIPresenter.Instance.FadeIn(0.1f);

            _judgementSystem.ForceCompleteAll();
            ClearAllNotes();
            presenter.ChangeTheme(theme);
            _eventSystem.SyncToTime(CurrentTime);

            await GlobalUIPresenter.Instance.FadeOut(0.1f);
            InputManager.Instance.SetBlockInput(false);
            _isThemeChanging = false;
        }

        private void ClearAllNotes()
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (note == null) continue;

                if (note.IsPersistent)
                {
                    note.gameObject.SetActive(false);
                    continue;
                }

                Destroy(note.gameObject);
            }

            _activeNotes.RemoveAll(n => n == null || !n.IsPersistent);
        }

        private void OnDestroy()
        {
            _inputController?.Dispose();
            _audioTimeline?.Stop();
        }
    }
}