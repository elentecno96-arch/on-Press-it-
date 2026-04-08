using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Project.Core.Managers
{
    /// <summary>
    /// 인풋 시스템을 관리하는 매니저 (멀티 터치 및 개별 손가락 추적 지원)
    /// </summary>
    public class InputManager : BaseSingleton<InputManager>
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

            // UI를 터치한 손가락은 추적 목록에 넣지 않고 무시합니다.
            if (IsPointerOverUI(finger.currentTouch.screenPosition))
            {
                _lastInputType = "UI_TOUCHED";
                return;
            }

            // [수정됨] 게임 영역을 터치한 유효한 손가락을 등록합니다.
            _validGameTouches.Add(finger);
            _lastInputType = "DOWN";

            OnPointerDown?.Invoke(finger.currentTouch.screenPosition);
        }

        private void OnFingerMove(Finger finger)
        {
            if (_isInputBlocked) return;

            // [수정됨] 유효한 게임 터치가 아니거나, 이미 슬라이드 처리된 손가락이면 무시합니다.
            if (!_validGameTouches.Contains(finger) || _slideProcessedTouches.Contains(finger)) return;

            if (finger.currentTouch.delta.magnitude > SLIDE_THRESHOLD)
            {
                _lastInputType = "SLIDE";
                _slideProcessedTouches.Add(finger); // 이 손가락은 슬라이드 처리됨을 기록
                OnSlideAction?.Invoke(finger.currentTouch.delta);
            }
        }

        private void OnFingerUp(Finger finger)
        {
            // [수정됨] 떼어진 손가락이 우리가 추적하던 '게임 영역 터치' 손가락인지 확인합니다.
            if (_validGameTouches.Contains(finger))
            {
                _validGameTouches.Remove(finger);
                _slideProcessedTouches.Remove(finger); // 슬라이드 상태도 초기화

                if (!_isInputBlocked)
                {
                    _lastInputType = "UP";
                    OnPointerUp?.Invoke();
                }
            }
        }

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogWarning("<color=red>[InputManager]</color> 씬에 EventSystem이 없습니다!");
                return false;
            }

            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = screenPosition
            };
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
                // 인풋이 차단될 때 잡고 있던 모든 홀드를 강제로 풀어줍니다.
                if (_validGameTouches.Count > 0)
                {
                    OnPointerUp?.Invoke();
                }

                _validGameTouches.Clear();
                _slideProcessedTouches.Clear();

                _lastInputType = "BLOCKED";
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