using Project.Core.Managers;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using System;
using UnityEngine;

namespace Project.Rhythm.Judgement
{
    /// <summary>
    /// 입력을 판정 시스템으로 전달하는 중계기
    /// </summary>
    public class RhythmInputController : IDisposable
    {
        private readonly JudgementSystem _judgement;
        private readonly Func<float> _getCurrentTime;
        public event Action<PatternType> OnInputTriggered;

        public RhythmInputController(JudgementSystem judgement, Func<float> timeProvider)
        {
            _judgement = judgement;
            _getCurrentTime = timeProvider;

            BindEvents();
        }

        private void BindEvents()
        {
            var input = InputManager.Instance;
            if (input == null) return;

            input.OnPointerDown += PointerDown;
            input.OnPointerUp += PointerUp;
            input.OnSlideAction += Slide;
        }

        private void PointerDown(Vector2 pos)
        {
            float currentTime = _getCurrentTime();

            _judgement.ProcessTap(currentTime);
            _judgement.ProcessHoldDown(currentTime);

            OnInputTriggered?.Invoke(PatternType.Tap);
        }

        private void PointerUp()
        {
            _judgement.ProcessHoldUp(_getCurrentTime());

            OnInputTriggered?.Invoke(PatternType.None);
        }

        private void Slide(Vector2 delta)
        {
            float currentTime = _getCurrentTime();

            _judgement.ProcessSlide(currentTime);

            OnInputTriggered?.Invoke(PatternType.Slide);
        }

        public void Dispose()
        {
            var input = InputManager.Instance;
            if (input == null) return;

            input.OnPointerDown -= PointerDown;
            input.OnPointerUp -= PointerUp;    
            input.OnSlideAction -= Slide;      

            Debug.Log("<color=yellow>[RhythmInputController]</color> 모든 입력을 안전하게 해제했습니다.");
        }
    }
}

