using Project.Rhythm.Data;
using Project.Rhythm.Judgement;
using Project.Rhythm.Note.Interface;
using UnityEngine;

namespace Project.Rhythm.Note.State
{
    /// <summary>
    /// 노트 행동 상태
    /// </summary>
    public class ActionState : INoteState
    {
        private Note _note;
        private bool _isJudged;

        public ActionState(Note note)
        {
            _note = note;
        }

        public void Enter()
        {
            _isJudged = false;
        }

        public void Update(float currentTime)
        {
            float timeToHit = _note.TargetTime - currentTime;
            float progress = Mathf.Clamp01(1f - (timeToHit / _note.AppearDuration));

            _note.MoveStrategy?.UpdateMove(_note.transform, progress);

            if (progress >= 1.2f && !_isJudged)
            {
                _isJudged = true;
                _note.ChangeState(new EndState(_note, JudgeResult.Miss));
            }
        }

        public void HandleInput(PatternType inputType)
        {
            if (_isJudged) return;

            _note.ExecuteAction(inputType);

            _isJudged = true;
        }

        public void Exit()
        {
            // 상태 종료 시 리소스 정리
        }
    }
}