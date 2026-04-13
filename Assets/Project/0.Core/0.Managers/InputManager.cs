using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using Project.Rhythm.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Project.Core.Managers
{
    /// <summary>
    /// 인풋 시스템을 관리하는 매니저 (멀티 터치 및 개별 손가락 추적 지원)
    /// (리펙토링) UI 터치 구분 하는 로직을 유틸리티로 분리
    /// (버그수정) 고스트 터치 방지를 위한 Index 기반 추적 및 Active Polling 적용
    /// </summary>
    public class InputManager : BaseSingleton<InputManager>, IInputProvider
    {
        public event Action<Vector2> OnPointerDown; // Tap/Hold 시작 통합
        public event Action<Vector2> OnSlideAction;
        public event Action OnPointerUp;
        private bool _isInputBlocked = false;

        // 화면에 닿은 전체 터치가 아니라, '게임 영역'을 터치한 유효 손가락만 카운트합니다.
        public bool IsPressing => IsInitialized && !_isInputBlocked && _validGameTouches.Count > 0;

        private const float SLIDE_THRESHOLD = 50f;
        private string _lastInputType = "None";

        // Finger 객체 대신 고유 번호(index)로 추적하여 Object Pooling 버그 차단
        private readonly HashSet<int> _validGameTouches = new HashSet<int>();
        private readonly HashSet<int> _slideProcessedTouches = new HashSet<int>();

        // 가비지 컬렉터용 리스트 (Update문 내 메모리 할당 방지)
        private readonly List<int> _deadTouchesBuffer = new List<int>(10);

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

        // 이벤트 누락(OS 인터럽트 등)으로 발생한 고스트 터치를 매 프레임 감지하고 청소
        private void Update()
        {
            if (!IsInitialized || _isInputBlocked || _validGameTouches.Count == 0) return;

            bool isGhostRemoved = false;
            _deadTouchesBuffer.Clear();

            // 1. 우리가 누르고 있다고 판단하는 터치 ID들을 순회
            foreach (int fingerIndex in _validGameTouches)
            {
                bool isAlive = false;

                // 2. 유니티가 현재 인식 중인 실제 터치 목록과 교차 검증
                foreach (var activeTouch in Touch.activeTouches)
                {
                    if (activeTouch.finger.index == fingerIndex)
                    {
                        // 🔴 [Critical Fix] 네임스페이스 충돌을 막기 위해 UnityEngine.InputSystem을 명시적으로 적어줍니다.
                        if (activeTouch.phase != UnityEngine.InputSystem.TouchPhase.Ended &&
                            activeTouch.phase != UnityEngine.InputSystem.TouchPhase.Canceled)
                        {
                            isAlive = true;
                        }
                        break;
                    }
                }

                // 3. 실제로는 화면에 없는데 리스트에 남아있다면 삭제 버퍼에 추가
                if (!isAlive)
                {
                    _deadTouchesBuffer.Add(fingerIndex);
                }
            }

            // 4. 발견된 고스트 터치들을 강제 청소
            foreach (int deadId in _deadTouchesBuffer)
            {
                _validGameTouches.Remove(deadId);
                _slideProcessedTouches.Remove(deadId);
                isGhostRemoved = true;
                Debug.LogWarning($"<color=orange>[InputManager] 고스트 터치 강제 해제됨 (Finger Index: {deadId})</color>");
            }

            // 5. 청소된 터치가 있다면 강제로 OnPointerUp 이벤트를 호출하여 게임 내 홀드를 해제
            if (isGhostRemoved && !_isInputBlocked)
            {
                _lastInputType = "UP_GHOST_CLEARED";
                OnPointerUp?.Invoke();
            }
        }

        private void OnFingerDown(Finger finger)
        {
            if (_isInputBlocked) return;

            if (UIUtils.IsPointerOverUI(finger.currentTouch.screenPosition))
            {
                _lastInputType = "UI_TOUCHED";
                return;
            }

            // Finger 객체가 아닌 고유 index 저장
            _validGameTouches.Add(finger.index);
            _lastInputType = "DOWN";
            OnPointerDown?.Invoke(finger.currentTouch.screenPosition);
        }

        private void OnFingerMove(Finger finger)
        {
            if (_isInputBlocked || !_validGameTouches.Contains(finger.index)) return;
            if (_slideProcessedTouches.Contains(finger.index)) return;

            if (finger.currentTouch.delta.magnitude > SLIDE_THRESHOLD)
            {
                _lastInputType = "SLIDE";
                _slideProcessedTouches.Add(finger.index);
                OnSlideAction?.Invoke(finger.currentTouch.delta);
            }
        }

        private void OnFingerUp(Finger finger)
        {
            // Finger 객체가 아닌 고유 index로 삭제 시도
            if (_validGameTouches.Remove(finger.index))
            {
                _slideProcessedTouches.Remove(finger.index);

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

        // 전화가 오거나 앱이 백그라운드로 전환될 때 강제 초기화
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SetBlockInput(true);
                SetBlockInput(false); // 막은 후 다시 해제하여 꼬임 방지
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SetBlockInput(true);
                SetBlockInput(false);
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