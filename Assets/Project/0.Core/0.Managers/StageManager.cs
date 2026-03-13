using Cysharp.Threading.Tasks;
using Project.Rhythm;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
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
        public NoteCollection NoteCollection { get; private set; }

        [SerializeField] private float noteAppearDuration = 2.0f;
        [SerializeField] private float stageStartDelay = 1.5f;

        private AudioTimeline _audioTimeline;
        private RhythmEventSystem _eventSystem;
        private JudgementSystem _judgementSystem;
        private NoteSpawner _noteSpawner;
        private ITouchVisual _touchVisual; 

        private bool _isInitialized;
        private StageData _activeStageData;

        public event Action OnStageStart;
        public event Action OnStageComplete;
        private bool _isPressing;

        private readonly List<Note> _activeNotes = new();
        private StoneVisual _fixedStone;

        public static float CurrentTime { get; private set; }

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            InitializeSystems(_activeStageData);

            presenter.Initialize(_activeStageData);
            _noteSpawner = new NoteSpawner(presenter);
            _touchVisual = presenter.GetTouchVisual();

            CollectPersistentNotes();

            BindSystems();
            AddInputEvents();

            _isInitialized = true;

            StartSequence(_activeStageData, this.GetCancellationTokenOnDestroy()).Forget();

            await UniTask.CompletedTask;
        }

        private void InitializeSystems(StageData stageData)
        {
            NoteCollection = new NoteCollection();

            _audioTimeline = new AudioTimeline();
            _audioTimeline.Initialize(musicSource, stageData);

            _eventSystem = new RhythmEventSystem();
            _eventSystem.Initialize(stageData, noteAppearDuration);

            _judgementSystem = new JudgementSystem();
            _judgementSystem.Initialize(stageData);
        }

        private void BindSystems()
        {
            _judgementSystem.OnJudged += (result, note) =>
            {
                _touchVisual?.PlayAction(result);

                if (note != null)
                {
                    note.OnJudged(result);
                }
            };

            _eventSystem.OnSpawnTriggered += (action, hitTime) =>
            {
                var persistentNote = NoteCollection.GetNote("Stage3_Stone");

                if (action.type == PatternType.Hold && persistentNote != null)
                {
                    _judgementSystem.RegisterNote(action, persistentNote);
                }
                else
                {
                    Note spawnedNote = _noteSpawner.Spawn(CurrentTime, noteAppearDuration);
                    if (spawnedNote != null)
                    {
                        _judgementSystem.RegisterNote(action, spawnedNote);
                        _activeNotes.Add(spawnedNote);
                    }
                }
            };
        }

        private void CollectPersistentNotes()
        {
            if (presenter == null) return;

            var allNotes = presenter.GetComponentsInChildren<Note>(true);
            foreach (var note in allNotes)
            {
                if (note.IsPersistent && !string.IsNullOrEmpty(note.NoteID))
                {
                    NoteCollection.Register(note.NoteID, note);
                }
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;

            CurrentTime = _audioTimeline.GetStageTime();
            if (CurrentTime < 0f) return;

            _eventSystem.Process(CurrentTime);
            _judgementSystem.UpdateHoldCheck(_isPressing, CurrentTime);
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

        #region Input Handling
        private void OnTap(Vector2 pos)
        {
            if (!_isInitialized) return;
            _judgementSystem.ProcessInput(PatternType.Tap, CurrentTime);
            _touchVisual?.PlayAction(PatternType.Tap);
        }

        private void OnSlide(Vector2 delta)
        {
            if (!_isInitialized) return;
            _judgementSystem.ProcessInput(PatternType.Slide, CurrentTime);
            _touchVisual?.PlayAction(PatternType.Slide);
        }

        private void OnHold()
        {
            if (!_isInitialized) return;
            _isPressing = true; 

            _judgementSystem.ProcessInputDown(CurrentTime);
            _touchVisual?.PlayAction(PatternType.Hold);
        }

        private void OnRelease()
        {
            if (!_isInitialized) return;
            _isPressing = false; 

            _judgementSystem.ProcessInputUp(CurrentTime);

            if (_touchVisual is ITouchVisual visual) visual.StopHoldAction();
        }
        #endregion

        #region Lifecycle & Events
        private void AddInputEvents()
        {
            var input = InputManager.Instance;
            if (input == null) return;
            input.OnTapAction += OnTap;
            input.OnSlideAction += OnSlide;
            input.OnHoldAction += OnHold;
            input.OnReleaseAction += OnRelease;
        }

        private void RemoveInputEvents()
        {
            var input = InputManager.Instance;
            if (input == null) return;
            input.OnTapAction -= OnTap;
            input.OnSlideAction -= OnSlide;
            input.OnHoldAction -= OnHold;
            input.OnReleaseAction -= OnRelease;
        }

        private void OnDestroy()
        {
            RemoveInputEvents();
        }
        #endregion
    }
}