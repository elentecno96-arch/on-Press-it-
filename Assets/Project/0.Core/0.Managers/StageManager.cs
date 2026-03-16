using Cysharp.Threading.Tasks;
using Project.Rhythm;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Event;
using Project.Rhythm.Interface;
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
        private ITouchVisual _touchVisual;

        private RhythmInputController _inputController;

        private bool _isInitialized;
        private StageData _activeStageData;

        public event Action OnStageStart;
        public event Action OnStageComplete;

        private readonly List<Note> _activeNotes = new();
        private StoneVisual _fixedStone;

        public static float CurrentTime { get; private set; }

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            InitializeSystems(_activeStageData);

            presenter.Initialize(_activeStageData);
            _noteSpawner = new NoteSpawnSystem(presenter);
            _touchVisual = presenter.GetTouchVisual();

            _inputController = new RhythmInputController(_judgementSystem, () => CurrentTime);

            _inputController.OnInputTriggered += (type) =>
            {
                if (type == PatternType.None) _touchVisual?.StopHoldAction();
                else _touchVisual?.PlayAction(type);
            };

            BindSystems();

            _judgementSystem.OnJudged += (result, note) => {
                presenter.GetTouchVisual()?.PlayAction(result);
            };

            _isInitialized = true;
            StartSequence(_activeStageData, this.GetCancellationTokenOnDestroy()).Forget();

            await UniTask.CompletedTask;
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
            _judgementSystem.OnJudged += (result, note) => {
                _touchVisual?.PlayAction(result);
                if (note != null)
                {
                    note.OnJudged(result);
                }
            };

            _eventSystem.OnSpawnTriggered += (action, hitTime, duration) =>
            {
                var note = _noteSpawner.GetOrSpawn(action, CurrentTime, duration);

                if (note == null) return;

                if (action.role == ActionRole.Signal)
                {
                    note.PlaySignalEffect();
                }
                else
                {
                    _judgementSystem.RegisterNote(action, note);
                    if (!note.IsPersistent) _activeNotes.Add(note);
                }
            };
        }

        private void Update()
        {
            if (!_isInitialized) return;

            CurrentTime = _audioTimeline.GetStageTime();
            if (CurrentTime < 0f) return;

            _eventSystem.Process(CurrentTime);
            _judgementSystem.UpdateHoldCheck(InputManager.Instance.IsPressing, CurrentTime);
            _judgementSystem.CheckMiss(CurrentTime);

            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (note == null)
                {
                    _activeNotes.RemoveAt(i);
                    continue;
                }

                note.UpdateNote(CurrentTime);
            }

            if (_judgementSystem.IsHolding)
            {
                float progress = _judgementSystem.GetHoldProgress(CurrentTime);

                _touchVisual?.UpdateVisual(progress);

                var holdNote = _judgementSystem.GetCurrentHoldNote();
                if (holdNote != null)
                {
                    holdNote.UpdateHoldProgress(progress);
                }
            }
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

                OnStageComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
        }

        private void OnDestroy()
        {
            _inputController?.Dispose(); // 구독 해제
        }
    }
}