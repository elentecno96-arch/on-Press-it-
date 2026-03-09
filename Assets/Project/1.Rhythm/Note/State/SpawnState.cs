using Project.Rhythm.Data;
using Project.Rhythm.Note.Interface;

namespace Project.Rhythm.Note.State
{
    /// <summary>
    /// 노트의 생성 상태
    /// </summary>
    public class SpawnState : INoteState
    {
        private Note _note;
        public SpawnState(Note note) => _note = note;

        public void Enter()
        {
            _note.SpawnStrategy?.SetPosition(_note.transform);

            _note.ChangeState(new MoveState(_note));
        }
        public void Update(float currentTime) { }
        public void HandleInput(PatternType type) { }
        public void Exit() { }
    }
}
