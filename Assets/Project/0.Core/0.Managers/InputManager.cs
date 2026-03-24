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
        private bool _isInputBlocked = false;

        public bool IsPressing => !_isInputBlocked && Touch.activeTouches.Count > 0;

        private const float SLIDE_THRESHOLD = 50f;
        private string _lastInputType = "None";
        private bool _isSlideProcessed = false;

        private bool _ignoreGameInput = false;

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
            if (_isInputBlocked) return;

            if (IsPointerOverUI(finger.currentTouch.screenPosition))
            {
                _ignoreGameInput = true;
                _lastInputType = "UI_TOUCHED";
                return;
            }

            _ignoreGameInput = false;
            _lastInputType = "DOWN";
            _isSlideProcessed = false;
            OnPointerDown?.Invoke(finger.currentTouch.screenPosition);
        }

        private void OnFingerMove(Finger finger)
        {
            if (_isInputBlocked || _ignoreGameInput || _isSlideProcessed) return;

            if (finger.currentTouch.delta.magnitude > SLIDE_THRESHOLD)
            {
                _lastInputType = "SLIDE";
                _isSlideProcessed = true;
                OnSlideAction?.Invoke(finger.currentTouch.delta);
            }
        }

        private void OnFingerUp(Finger finger)
        {
            if (!_isInputBlocked && !_ignoreGameInput)
            {
                _lastInputType = "UP";
                OnPointerUp?.Invoke();
            }
            _ignoreGameInput = false;
        }

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogWarning("<color=red>[InputManager]</color> 씬에 EventSystem이 없습니다!");
                return false;
            }

            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            eventData.position = screenPosition;
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    Debug.Log($"<color=yellow>[UI Hit]</color> Object Name: <b>{result.gameObject.name}</b> | Layer: {LayerMask.LayerToName(result.gameObject.layer)}");
                }
            }

            return results.Count > 0;
        }

        /// <summary>
        /// 인풋 초기화 및 차단 설정
        /// </summary>
        /// <param name="block"></param>
        public void SetBlockInput(bool block)
        {
            _isInputBlocked = block;
            if (block)
            {
                OnPointerUp?.Invoke();
                _lastInputType = "BLOCKED";
                _isSlideProcessed = false;
                _ignoreGameInput = false;
            }
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