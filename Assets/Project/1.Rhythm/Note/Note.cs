using Project.Core.Managers;
using Project.Rhythm.Data;
using Project.Rhythm.Note.Interface;
using Project.Rhythm.Note.State;
using UnityEngine;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 베이스 노트 ( 필드의 모든 노트는 이 스크립트를 컴포넌트 해야함 )
    /// </summary>
    public class Note : MonoBehaviour
    {
        public ISpawnStrategy SpawnStrategy { get; private set; }
        public IMoveStrategy MoveStrategy { get; private set; }
        public IActionStrategy ActionStrategy { get; private set; }

        private TouchEventVisual _visual;

        public float TargetTime { get; private set; }
        public float AppearDuration { get; private set; }
        public float SpawnTime { get; private set; }

        private INoteState _currentState;
        public INoteState CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        private void Awake()
        {
            _visual = GetComponent<TouchEventVisual>();
        }

        public void Setup(ISpawnStrategy s, IMoveStrategy m, IActionStrategy a, float target, float duration)
        {
            SpawnStrategy = s;
            MoveStrategy = m;
            ActionStrategy = a;
            TargetTime = target;
            AppearDuration = duration;

            SpawnTime = StageManager.CurrentTime;

            ChangeState(new SpawnState(this));
        }

        public void ChangeState(INoteState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void ExecuteAction(PatternType type)
        {
            ActionStrategy?.Execute(_visual, type);
        }

        public void UpdateNote(float currentTime)
        {
            _currentState?.Update(currentTime);
        }

        public void OnInput(PatternType type)
        {
            _currentState?.HandleInput(type);
        }
    }
}
