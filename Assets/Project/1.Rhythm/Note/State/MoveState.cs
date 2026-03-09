using Project.Rhythm.Data;
using Project.Rhythm.Judgement;
using Project.Rhythm.Note.Interface;
using UnityEngine;

namespace Project.Rhythm.Note.State
{
    /// <summary>
    /// 노트의 이동 상태
    /// </summary>
    public class MoveState : INoteState
    {
        private Note _note;
        public MoveState(Note note) => _note = note;

        public void Enter() { }

        public void Update(float currentTime)
        {
            float elapsed = currentTime - _note.SpawnTime;

            float progress = Mathf.Clamp01(elapsed / _note.AppearDuration);

            _note.MoveStrategy?.UpdateMove(_note.transform, progress);

            if (progress >= 0.7f && _note.CurrentState is MoveState)
            {
                _note.ChangeState(new ActionState(_note));
            }

            if (progress >= 1.0f)
            {
                _note.ChangeState(new EndState(_note, JudgeResult.Miss));
            }
        }

        public void HandleInput(PatternType type)
        {
            _note.ExecuteAction(type);
        }

        public void Exit() { }
    }
}
