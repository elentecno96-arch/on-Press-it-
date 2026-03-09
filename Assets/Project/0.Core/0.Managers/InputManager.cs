using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Project.Core.Managers
{
    /// <summary>
    /// 인풋 시스템을 관리하는 매니저 (이벤트 기반 완성본)
    /// </summary>
    public class InputManager : BaseSingleton<InputManager>
    {
        private GameInput _gameInput;

        public event Action<Vector2> OnTapAction;
        public event Action<Vector2> OnSlideAction;
        public event Action OnHoldAction;
        public event Action OnReleaseAction;

        private const float SLIDE_THRESHOLD = 50f;
        private const float HOLD_DETECTION_TIME = 0.2f;

        private float _touchStartTime;
        private bool _isHoldLogged = false;
        private string _lastInputType = "None"; //런타임 디버깅 용 변수

        public override UniTask Initialize()
        {
            if (IsInitialized) return UniTask.CompletedTask;

            _gameInput = new GameInput();
            EnhancedTouchSupport.Enable(); // 다중 터치 지원 활성화
            _gameInput.Player.Enable();

            IsInitialized = true;
            Debug.Log("<color=green>[InputManager]</color> 초기화 완료");
            return UniTask.CompletedTask;
        }

        private void Update()
        {
            if (!IsInitialized) return;

            var activeTouches = Touch.activeTouches;
            if (activeTouches.Count == 0) return;

            var touch = activeTouches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _lastInputType = "TAP"; //탭 기록
                    _touchStartTime = Time.time;
                    _isHoldLogged = false;
                    OnTapAction?.Invoke(touch.screenPosition);
                    break;

                case TouchPhase.Moved:
                    if (touch.delta.magnitude > SLIDE_THRESHOLD)
                    {
                        _lastInputType = "SLIDE"; //슬라이드 기록
                        OnSlideAction?.Invoke(touch.delta);
                    }
                    break;

                case TouchPhase.Stationary:
                    if (Time.time - _touchStartTime > HOLD_DETECTION_TIME && !_isHoldLogged)
                    {
                        _lastInputType = "HOLD"; //홀드 기록
                        OnHoldAction?.Invoke();
                        _isHoldLogged = true;
                    }
                    break;
                case TouchPhase.Ended:
                    OnReleaseAction?.Invoke();
                    break;
            }
        }

        private void OnDisable()
        {
            _gameInput?.Player.Disable();
            EnhancedTouchSupport.Disable();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            GUI.Label(new Rect(20, 100, 400, 50), $"Last Input: {_lastInputType}", new GUIStyle { fontSize = 30, normal = new GUIStyleState { textColor = Color.cyan } });
        }
#endif
    }


}