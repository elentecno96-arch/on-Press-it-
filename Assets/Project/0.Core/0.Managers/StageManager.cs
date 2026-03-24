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

        private readonly List<Project.Rhythm.Note.Note> _activeNotes = new(); // 네임스페이스 명시
        public static float CurrentTime { get; private set; }

        private List<(float time, StageThemeType theme)> _themeQueue = new();
        private int _themeIndex = 0;
        private bool _isThemeChanging = false;

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            InitializeSystems(_activeStageData);
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
                presenter.ShowJudgeEffect(result);
            };

            _eventSystem.OnSpawnTriggered += (action, hitTime, duration) =>
            {
                Project.Rhythm.Note.Note note = null;

                if (!string.IsNullOrEmpty(action.targetID))
                {
                    note = presenter.GetFixedNote(action.targetID);
                    if (note != null)
                    {
                        note.ResetJudgedState();
                        note.InitializePersistent(CurrentTime, duration);
                    }
                }

                if (note == null)
                {
                    var noteObj = _noteSpawner.GetOrSpawn(action, CurrentTime, duration);
                    note = noteObj?.GetComponent<Project.Rhythm.Note.Note>();
                }

                if (note == null) return;

                _judgementSystem.RegisterNote(action, note);
                if (!note.IsPersistent && !_activeNotes.Contains(note))
                {
                    _activeNotes.Add(note);
                }
            };
        }

        private void Update()
        {
            if (!_isInitialized) return;

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

            // 이미지 3번 에러 해결 구간
            if (_judgementSystem.IsHolding)
            {
                float progress = _judgementSystem.GetHoldProgress(CurrentTime);
                presenter.GetTouchVisual()?.UpdateVisual(progress);

                // .UpdateHoldProgress 메서드가 Note 클래스에 있는지 확인 필요
                _judgementSystem.GetCurrentHoldNote()?.UpdateHoldProgress(progress);
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

                // ★ [기록 저장 핵심 추가]
                // 1. 점수 계산 및 PlayerManager 저장 실행
                _judgementSystem.FinalizeAndSaveResult();

                // 2. UI 표시를 위한 데이터 가져오기
                int p = _judgementSystem.GetCount(JudgeResult.Perfect);
                int gr = _judgementSystem.GetCount(JudgeResult.Great);
                int go = _judgementSystem.GetCount(JudgeResult.Good);
                int m = _judgementSystem.GetCount(JudgeResult.Miss);

                presenter.ShowResult(p, gr, go, m);

                OnStageComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
        }

        // ... 이하 BuildThemeQueue, ProcessThemeChange, ChangeThemeWithFade, OnDestroy 동일
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
            if (CurrentTime >= next.time - 0.4f)
            {
                ChangeThemeWithFade(next.theme).Forget();
                _themeIndex++;
            }
        }

        private async UniTaskVoid ChangeThemeWithFade(StageThemeType theme)
        {
            if (_isThemeChanging) return;
            _isThemeChanging = true;
            await GlobalUIPresenter.Instance.FadeIn(1f);
            presenter.ChangeTheme(theme);
            await GlobalUIPresenter.Instance.FadeOut(0f);
            _isThemeChanging = false;
        }

        private void OnDestroy()
        {
            _inputController?.Dispose();
            _audioTimeline?.Stop();
        }
    }
}