using Cysharp.Threading.Tasks;
using Project.Core.Systems.Stage;
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
using UnityEngine;

namespace Project.Core.Managers
{
    public class StageManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StageData testStageData;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private StagePresenter presenter;

        [Header("Settings")]
        [SerializeField] private float noteAppearDuration = 2.0f;
        [SerializeField] private float stageStartDelay = 1.5f;

        // 핵심 시스템 부품들
        private AudioTimeline _audioTimeline;
        private RhythmEventSystem _eventSystem;
        private CountdownSystem _countdownSystem;
        private JudgementSystem _judgementSystem;
        private NoteSpawnSystem _noteSpawner;
        private RhythmInputController _inputController;

        // 분리된 전담 관리자들
        private StageFlow _stageFlow;
        private ThemeSwitcher _themeSwitcher;

        private bool _isInitialized;
        private StageData _activeStageData;
        private bool _isThemeChanging = false;

        private readonly List<Note> _activeNotes = new();
        private List<(float time, StageThemeType theme)> _themeQueue = new();
        private int _themeIndex = 0;

        public event Action OnStageStart;
        public event Action OnStageComplete;

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            InitializeSystems(_activeStageData);

            presenter.SetJudgementSystem(_judgementSystem);
            presenter.Initialize(_activeStageData, _audioTimeline);

            BuildThemeQueue(_activeStageData);
            _noteSpawner = new NoteSpawnSystem(presenter);

            BindSystems();

            _isInitialized = true;
            await UniTask.CompletedTask;
        }

        private void InitializeSystems(StageData stageData)
        {
            _audioTimeline = new AudioTimeline();
            _audioTimeline.Initialize(musicSource, stageData.masterTrack, stageData.playStartTime);

            _eventSystem = new RhythmEventSystem(_audioTimeline);
            _eventSystem.Initialize(stageData, noteAppearDuration);

            _countdownSystem = new CountdownSystem(_audioTimeline);
            _countdownSystem.Initialize(stageData);

            _judgementSystem = new JudgementSystem(_audioTimeline);
            _judgementSystem.Initialize(stageData);

            _inputController = new RhythmInputController(_judgementSystem, InputManager.Instance);
            _inputController.OnInputTriggered += InputTriggered;

            _stageFlow = new StageFlow(_judgementSystem, _audioTimeline, presenter);
            _themeSwitcher = new ThemeSwitcher(_judgementSystem, _eventSystem, _countdownSystem, _audioTimeline, presenter);
        }

        private void BindSystems()
        {
            _countdownSystem.OnCountdownTriggered += (targetBeat) => presenter.StartCountdown(targetBeat);

            _judgementSystem.OnJudged += (result, note) =>
            {
                presenter.GetTouchVisual()?.PlayAction(result);
                note?.OnJudged(result);
            };

            _eventSystem.OnSpawnTriggered += (action, hitTime, duration) =>
            {
                float currentTime = _audioTimeline.GetStageTime();
                switch (action.noteType)
                {
                    case NoteType.Signal: HandleSignal(action); break;
                    case NoteType.Persistent: HandlePersistent(action, currentTime, duration); break;
                    case NoteType.Runtime: HandleRuntime(action, currentTime, duration); break;
                }
            };
        }

        public void Play()
        {
            if (!_isInitialized) return;
            OnStageStart?.Invoke();

            PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            try
            {
                await _stageFlow.PlaySequence(_activeStageData, stageStartDelay, this.GetCancellationTokenOnDestroy());

                OnStageComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Stage Play Canceled");
            }
        }

        private void Update()
        {
            // 테마 변경 중에는 모든 업데이트 로직을 정지
            if (!_isInitialized || _isThemeChanging) return;

            float currentTime = _audioTimeline.GetStageTime();
            if (currentTime < 0f) return;

            // 각 시스템에 Process 신호 전파
            presenter.UpdateUI(currentTime);
            ProcessThemeChange(currentTime);

            _eventSystem.Process();
            _countdownSystem.Process();
            _judgementSystem.UpdateHoldCheck(InputManager.Instance.IsPressing);
            _judgementSystem.CheckMiss();

            UpdateActiveNotes(currentTime);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_judgementSystem.IsHolding)
            {
                presenter.GetTouchVisual()?.UpdateVisual(_judgementSystem.GetHoldProgress());
            }
        }

        private void UpdateActiveNotes(float currentTime)
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (note == null) { _activeNotes.RemoveAt(i); continue; }
                note.UpdateNote(currentTime);
            }
        }

        private void ProcessThemeChange(float currentTime)
        {
            if (_themeIndex >= _themeQueue.Count) return;

            if (currentTime >= _themeQueue[_themeIndex].time - 0.1f)
            {
                var theme = _themeQueue[_themeIndex].theme;
                _isThemeChanging = true;

                // 테마 스위칭 책임을 ThemeSwitcher에게 위임
                _themeSwitcher.Switch(theme, () => {
                    ClearAllNotes();
                    _isThemeChanging = false;
                }, this.GetCancellationTokenOnDestroy()).Forget();

                _themeIndex++;
            }
        }

        #region Note Handling
        private void HandleRuntime(RhythmAction action, float currentTime, float duration)
        {
            var noteObj = _noteSpawner.GetOrSpawn(action, currentTime, duration);
            var note = noteObj?.GetComponent<Note>();
            if (note == null) return;

            note.UpdateNote(currentTime);
            _judgementSystem.RegisterNote(action, note);

            if (!_activeNotes.Contains(note)) _activeNotes.Add(note);
        }

        private void HandlePersistent(RhythmAction action, float currentTime, float duration)
        {
            Note note = presenter.GetOrSpawnPersistent(action.targetID);
            if (note == null) return;

            note.gameObject.SetActive(true);
            note.ResetJudgedState();
            note.InitializePersistent(currentTime, duration);
            note.UpdateNote(currentTime);

            _judgementSystem.RegisterNote(action, note);
            if (!_activeNotes.Contains(note)) _activeNotes.Add(note);
        }

        private void HandleSignal(RhythmAction action)
        {
            presenter.GetFixedNote(action.targetID)?.PlaySignal();
        }

        private void ClearAllNotes()
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (note == null) continue;

                if (note.IsPersistent) note.gameObject.SetActive(false);
                else Destroy(note.gameObject);
            }
            _activeNotes.RemoveAll(n => n == null || !n.IsPersistent);
        }
        #endregion

        private void InputTriggered(PatternType type)
        {
            var visual = presenter.GetTouchVisual();
            if (type == PatternType.None) visual?.StopHoldAction();
            else visual?.PlayAction(type);
        }

        private void BuildThemeQueue(StageData data)
        {
            _themeQueue.Clear();
            _themeIndex = 0;
            if (data.themeEvents == null) return;

            float bpm = data.bpm;
            foreach (var evt in data.themeEvents)
                _themeQueue.Add((evt.beat * (60f / bpm), evt.theme));
        }

        private void OnDestroy()
        {
            if (_inputController != null)
            {
                _inputController.OnInputTriggered -= InputTriggered;
                _inputController.Dispose();
            }
            _audioTimeline?.Stop();
        }
    }
}