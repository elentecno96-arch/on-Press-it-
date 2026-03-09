using Project.Rhythm.Data;
using Project.Rhythm.Judgement;
using Project.Rhythm.Note.Interface;
using UnityEngine;

namespace Project.Rhythm.Note.State
{
    public class EndState : INoteState
    {
        private Note _note;
        private JudgeResult _result;
        private bool _isDisposed;

        public EndState(Note note, JudgeResult result)
        {
            _note = note;
            _result = result;
        }

        public void Enter()
        {
            Debug.Log($"Note End: {_result}");

            if (_result == JudgeResult.Miss)
            {
                ApplyMissVisual();
                Dispose();
            }
        }

        public void Update(float currentTime)
        {
            if (_isDisposed) return;

            if (_result != JudgeResult.Miss)
            {
                float elapsed = currentTime - _note.SpawnTime;
                float progress = Mathf.Clamp01(elapsed / _note.AppearDuration);

                _note.MoveStrategy?.UpdateMove(_note.transform, progress);

                if (progress >= 1.0f)
                {
                    Dispose();
                }
            }
        }

        public void HandleInput(PatternType type) { }
        public void Exit() { }

        private void ApplyMissVisual()
        {
            // Miss 시의 붉은색 변화 등 연출 로직
        }

        private void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_note != null && _note.gameObject != null)
            {
                Object.Destroy(_note.gameObject);
            }
        }
    }
}