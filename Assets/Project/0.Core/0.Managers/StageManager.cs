using Cysharp.Threading.Tasks;
using Project.Rhythm;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
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
        private StageData _activeStageData; //외부에서 받아온 SO를

        public event Action OnStageStart;
        public event Action OnStageComplete;

        private readonly List<Note> _activeNotes = new();
        public static float CurrentTime { get; private set; }

        public async UniTask Initialize()
        {
            if (_isInitialized) return;
            Debug.Log("스테이지 매니저 초기화 시작");
            //데이터 주입
            _activeStageData = GameManager.Instance.CurrentStageData ?? testStageData;

            //시스템 초기화
            InitializeSystems(_activeStageData);

            //비주얼 초기화
            presenter.Initialize(_activeStageData);

            _noteSpawner = new NoteSpawnSystem(presenter);

            _inputController = new RhythmInputController(_judgementSystem, () => CurrentTime);

            _inputController.OnInputTriggered += InputTriggered;

            BindSystems();

            _isInitialized = true;

            Debug.Log("스테이지 매니저 초기화 완료");

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
            _eventSystem.OnCountdownTriggered += (targetBeat) =>
            {
                presenter.StartCountdown(targetBeat);
            };

            _judgementSystem.OnJudged += (result, note) =>
            {
                presenter.GetTouchVisual()?.PlayAction(result);
                note?.OnJudged(result);
                presenter.ShowJudgeEffect(result); //판정 연출 ( 미구현 )
            };

            _eventSystem.OnSpawnTriggered += (action, hitTime, duration) =>
            {
                Note note = null;

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
                    note = noteObj?.GetComponent<Note>();
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

            presenter.UpdateUI(CurrentTime); //프로세스 바 

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
                _judgementSystem.GetCurrentHoldNote()?.UpdateHoldProgress(progress);
            }
        }

        private void InputTriggered(PatternType type)
        {
            var visual = presenter.GetTouchVisual();
            if (type == PatternType.None)
            {
                visual?.StopHoldAction();
            }
            else
            {
                visual?.PlayAction(type);
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

                int p = _judgementSystem.GetCount(JudgeResult.Perfect);
                int gr = _judgementSystem.GetCount(JudgeResult.Great);
                int go = _judgementSystem.GetCount(JudgeResult.Good);
                int m = _judgementSystem.GetCount(JudgeResult.Miss);

                presenter.ShowResult(p, gr, go, m);
                // ------------------------------------------

                OnStageComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
        }

        private void OnDestroy()
        {
            _inputController?.Dispose(); // 구독 해제
            _audioTimeline?.Stop();
        }
    }
}