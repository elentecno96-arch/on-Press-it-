using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Project.Core.Utilities;
using Cysharp.Threading.Tasks;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Project.Core.Managers
{
    /// <summary>
    /// 인풋 시스템을 관리하는 매니저
    /// </summary>
    public class InputManager : BaseSingleton<InputManager>
    {
        private GameInput _gameInput;

        // 슬라이드 판정을 위한 최소 거리
        private const float SLIDE_THRESHOLD = 50f;

        private float _touchStartTime;
        private bool _isHoldLogged = false;

        public override UniTask Initialize()
        {
            _gameInput = new GameInput();
            EnhancedTouchSupport.Enable(); //다중 터치
            _gameInput.Player.Enable();

            IsInitialized = true;
            Debug.Log("인풋 시스템 초기화 완료");
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
                    _touchStartTime = Time.time;
                    _isHoldLogged = false;
                    OnTap(touch.screenPosition); // 누른 순간 즉시 실행
                    break;

                case TouchPhase.Moved:
                    //움직임 거리가 임계값을 넘으면 슬라이드로 판정
                    if (touch.delta.magnitude > SLIDE_THRESHOLD)
                    {
                        OnSlide(touch.delta);
                    }
                    break;

                case TouchPhase.Stationary:
                    if (Time.time - _touchStartTime > 0.2f && !_isHoldLogged)
                    {
                        OnHold();
                        Debug.Log("Holding");
                        _isHoldLogged = true; // 한 번만 찍히게 방지
                    }
                    break;

                case TouchPhase.Ended: //터치를 끝냈을 때 0.2 전에 때면 Tap으로 인식
                    if (Time.time - _touchStartTime <= 0.2f)
                    {
                        Debug.Log("TapOut");
                    }
                    _isHoldLogged = false;
                    break;

                case TouchPhase.Canceled:
                    //홀드 종료
                    OnRelease();
                    break;
            }
        }

        private void OnTap(Vector2 pos) => Debug.Log("Tap!");

        private void OnSlide(Vector2 delta) => Debug.Log($"<color=cyan>[Slide]</color> 거리: {delta.magnitude:F2} | 방향: {delta.normalized}");

        private void OnHold()
        {
            Debug.Log("홀드중");
        }

        private void OnRelease() => Debug.Log("손가락 뗌");

        private void OnDisable()
        {
            _gameInput?.Player.Disable();
            EnhancedTouchSupport.Disable();
        }

#if UNITY_EDITOR
        private Vector2 _debugStartPos;
        private Vector2 _debugCurrentPos;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !EnhancedTouchSupport.enabled) return;

            var touches = Touch.activeTouches;
            foreach (var touch in touches)
            {
                Gizmos.color = touch.phase == TouchPhase.Began ? Color.red : Color.yellow;
                // 터치 지점에 원 그리기
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.screenPosition.x, touch.screenPosition.y, 10f));
                Gizmos.DrawWireSphere(worldPos, 0.5f);

                // 슬라이드 방향 선 그리기
                if (touch.phase == TouchPhase.Moved)
                {
                    Vector3 startWorld = Camera.main.ScreenToWorldPoint(new Vector3(touch.startScreenPosition.x, touch.startScreenPosition.y, 10f));
                    Gizmos.DrawLine(startWorld, worldPos);
                }
            }
        }
#endif
    }
}