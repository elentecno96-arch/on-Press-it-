using Cysharp.Threading.Tasks;
using Project.Rhythm;
using Project.Rhythm.Data;
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

        private AudioTimeline _audioTimeline;
        private RhythmEventSystem _eventSystem;
        private JudgementSystem _judgementSystem;
        private NoteSpawner _noteSpawner;
        private ITouchVisual _touchVisual;

        private bool _isInitialized;
        private StageData _activeStageData;

        public event Action OnStageStart;
        public event Action OnStageComplete;

        private readonly List<Note> _activeNotes = new List<Note>();

        // 시간의 절대 기준 (StageTime)
        public static float CurrentTime { get; private set; }

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            InitializeSystems(_activeStageData);

            presenter.Initialize(_activeStageData);
            _noteSpawner = new NoteSpawner(presenter);
            _touchVisual = presenter.GetTouchVisual();

            BindSystems();
            AddInputEvents();

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
            _eventSystem.OnSpawnTriggered += (action, hitTime) =>
            {
                Note spawnedNote = _noteSpawner.Spawn(action, hitTime, noteAppearDuration);

                if (spawnedNote != null)
                {
                    _judgementSystem.RegisterNote(action, spawnedNote);
                    _activeNotes.Add(spawnedNote);
                }
            };

            _judgementSystem.OnJudged += (result, note) =>
            {
                // note가 판정되면 리스트에서 제거 대상이 됩니다.
            };
        }

        private async UniTask StartSequence(StageData data, CancellationToken token)
        {
            try
            {
                await UniTask.Delay((int)(noteAppearDuration * 1000), cancellationToken: token);

                OnStageStart?.Invoke();
                _audioTimeline.StartTimeline();

                await UniTask.WaitUntil(
                    () => _audioTimeline.GetStageTime() >= (data.endPosition),
                    cancellationToken: token
                );

                OnStageComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
        }

        private void Update()
        {
            if (!_isInitialized) return;

            CurrentTime = _audioTimeline.GetStageTime();

            _eventSystem.Process(CurrentTime);
            _judgementSystem.CheckMiss(CurrentTime);

            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];

                if (note == null || !note.gameObject.activeInHierarchy)
                {
                    _activeNotes.RemoveAt(i);
                    continue;
                }
                note.UpdateNote(CurrentTime);
            }

            _eventSystem.Process(CurrentTime);
            _judgementSystem.CheckMiss(CurrentTime);
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
            _judgementSystem.ProcessInput(PatternType.Hold, CurrentTime);
            _touchVisual?.PlayAction(PatternType.Hold);
        }

        private void OnRelease()
        {
            if (!_isInitialized) return;
            if (_touchVisual is TouchEventVisual visual) visual.StopHoldAction();
        }

        #endregion

        #region Input Event Setup
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
            if (_eventSystem != null)
            {
                // 무명 메서드 구독 해제는 어려우므로 필요 시 별도 메서드로 추출
            }
        }
        #endregion
    }
}