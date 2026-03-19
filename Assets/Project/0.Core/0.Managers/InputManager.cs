using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Project.Core.Managers
{
    /// <summary>
    /// 인풋 시스템을 관리하는 매니저 (이벤트 기반 완성본)
    /// </summary>
    public class InputManager : BaseSingleton<InputManager>
    {
        private GameInput _gameInput;

        public event Action<Vector2> OnPointerDown; // Tap/Hold 시작 통합
        public event Action<Vector2> OnSlideAction;
        public event Action OnPointerUp;  

        public bool IsPressing => Touch.activeTouches.Count > 0;

        private const float SLIDE_THRESHOLD = 50f;
        private string _lastInputType = "None";
        private bool _isSlideProcessed = false;

        public override UniTask Initialize()
        {
            if (IsInitialized) return UniTask.CompletedTask;

            EnhancedTouchSupport.Enable();

            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerMove += OnFingerMove;
            Touch.onFingerUp += OnFingerUp;

            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        private void OnFingerDown(Finger finger)
        {
            _lastInputType = "DOWN";
            _isSlideProcessed = false;
            OnPointerDown?.Invoke(finger.currentTouch.screenPosition);
        }

        private void OnFingerMove(Finger finger)
        {
            if (_isSlideProcessed) return;

            if (finger.currentTouch.delta.magnitude > SLIDE_THRESHOLD)
            {
                _lastInputType = "SLIDE";
                _isSlideProcessed = true;
                OnSlideAction?.Invoke(finger.currentTouch.delta);
            }
        }

        private void OnFingerUp(Finger finger)
        {
            _lastInputType = "UP";
            OnPointerUp?.Invoke();
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerMove -= OnFingerMove; 
            Touch.onFingerUp -= OnFingerUp;

            EnhancedTouchSupport.Disable();

            OnPointerDown = null;
            OnSlideAction = null;
            OnPointerUp = null;

            Debug.Log("<color=red>[InputManager]</color> 모든 시스템 콜백 및 내부 이벤트를 초기화했습니다.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            GUI.Label(new Rect(20, 100, 400, 50), $"Input State: {_lastInputType} | Pressing: {IsPressing}",
                new GUIStyle { fontSize = 30, normal = new GUIStyleState { textColor = Color.cyan } });
        }
#endif
    }
}