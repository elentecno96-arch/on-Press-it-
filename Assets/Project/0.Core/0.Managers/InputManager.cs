using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using Project.Rhythm.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Project.Core.Managers
{
    /// <summary>
    /// 인풋 시스템을 관리하는 매니저 (멀티 터치 및 개별 손가락 추적 지원)
    /// (리펙토링) UI 터치 구분 하는 로직을 유틸리티로 분리
    /// </summary>
    public class InputManager : BaseSingleton<InputManager>, IInputProvider
    {
        public event Action<Vector2> OnPointerDown; // Tap/Hold 시작 통합
        public event Action<Vector2> OnSlideAction;
        public event Action OnPointerUp;
        private bool _isInputBlocked = false;

        // [수정됨] 화면에 닿은 전체 터치가 아니라, '게임 영역'을 터치한 유효 손가락만 카운트합니다.
        public bool IsPressing => IsInitialized && !_isInputBlocked && _validGameTouches.Count > 0;

        private const float SLIDE_THRESHOLD = 50f;
        private string _lastInputType = "None";

        // [추가됨] 각 손가락(Finger)의 상태를 개별적으로 추적하기 위한 컬렉션
        private readonly HashSet<Finger> _validGameTouches = new HashSet<Finger>();
        private readonly HashSet<Finger> _slideProcessedTouches = new HashSet<Finger>();

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

            if (UIUtils.IsPointerOverUI(finger.currentTouch.screenPosition))
            {
                _lastInputType = "UI_TOUCHED";
                return;
            }

            _validGameTouches.Add(finger);
            _lastInputType = "DOWN";
            OnPointerDown?.Invoke(finger.currentTouch.screenPosition);
        }

        private void OnFingerMove(Finger finger)
        {
            if (_isInputBlocked || !_validGameTouches.Contains(finger)) return;
            if (_slideProcessedTouches.Contains(finger)) return;

            if (finger.currentTouch.delta.magnitude > SLIDE_THRESHOLD)
            {
                _lastInputType = "SLIDE";
                _slideProcessedTouches.Add(finger);
                OnSlideAction?.Invoke(finger.currentTouch.delta);
            }
        }

        private void OnFingerUp(Finger finger)
        {
            if (_validGameTouches.Remove(finger))
            {
                _slideProcessedTouches.Remove(finger);

                if (!_isInputBlocked)
                {
                    _lastInputType = "UP";
                    OnPointerUp?.Invoke();
                }
            }
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
                if (_validGameTouches.Count > 0) OnPointerUp?.Invoke();
                _validGameTouches.Clear();
                _slideProcessedTouches.Clear();
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

            _validGameTouches.Clear();
            _slideProcessedTouches.Clear();

            Debug.Log("<color=red>[InputManager]</color> 모든 시스템 콜백 및 내부 이벤트를 초기화했습니다.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            GUI.Label(new Rect(20, 100, 500, 50),
                $"Input State: {_lastInputType} | Valid Touches: {_validGameTouches.Count} | Pressing: {IsPressing}",
                new GUIStyle { fontSize = 30, normal = new GUIStyleState { textColor = Color.cyan } });
        }
#endif
    }
}