using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using System;
using UnityEngine;

namespace Project.Rhythm.Judgement
{
    /// <summary>
    /// 입력을 판정 시스템으로 전달하는 중계기
    /// ( 리펙토링 ) 입력 이벤트를 직접 인풋 매니저에 가서 구독하는 대신
    /// 이 컨트롤러가 이벤트를 구독하여 판정 시스템에 전달하는 구조로 변경
    /// </summary>
    public class RhythmInputController : IDisposable
    {
        private readonly JudgementSystem _judgement;
        private readonly IInputProvider _inputProvider; // 인풋 매니저에서 이벤트를 구독하기 위한 인터페이스
        public event Action<PatternType> OnInputTriggered;

        public RhythmInputController(JudgementSystem judgement, IInputProvider inputProvider)
        {
            _judgement = judgement;
            _inputProvider = inputProvider;

            BindEvents();
        }

        private void BindEvents()
        {
            if (_inputProvider == null) //예외 처리 추가
            {
                Debug.LogError("[RhythmInputController] 주입된 InputProvider가 Null입니다!");
                return;
            }

            _inputProvider.OnPointerDown += PointerDown;
            _inputProvider.OnPointerUp += PointerUp;
            _inputProvider.OnSlideAction += Slide;
        }

        private void PointerDown(Vector2 pos)
        {
            _judgement.ProcessTap();
            _judgement.ProcessHoldDown();

            OnInputTriggered?.Invoke(PatternType.Tap);
        }

        private void PointerUp()
        {
            OnInputTriggered?.Invoke(PatternType.None);
        }

        private void Slide(Vector2 delta)
        {
            _judgement.ProcessSlide();

            OnInputTriggered?.Invoke(PatternType.Slide);
        }

        public void Dispose()
        {
            _inputProvider.OnPointerDown -= PointerDown;
            _inputProvider.OnPointerUp -= PointerUp;
            _inputProvider.OnSlideAction -= Slide;

            Debug.Log("<color=yellow>[RhythmInputController]</color> 모든 입력을 안전하게 해제했습니다.");
        }
    }
}

