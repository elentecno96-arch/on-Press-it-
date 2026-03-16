using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Managers;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core.Systems.Intro
{
    public class BootSequenceController : MonoBehaviour
    {
        private const float FADE_IN = 0.6f;
        private const float FADE_OUT = 0.5f;

        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup warningGroup;
        [SerializeField] private CanvasGroup logoGroup;
        [SerializeField] private CanvasGroup startGroup;

        [Header("Objects")]
        [SerializeField] private GameObject startObject;

        [Header("Animation Targets")]
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private RectTransform startTextTransform;

        private bool _isTransitioning;

        private async UniTaskVoid Start()
        {
            if (!CheckComponents()) return;

            await RunIntro();
        }

        private bool CheckComponents()
        {
            return warningGroup && logoGroup && startGroup && startObject;
        }

        private async UniTask RunIntro()
        {
            await GameManager.Instance.Initialize();

            InitUI();

            //Warning
            await warningGroup.DOFade(1, FADE_IN).AsyncWaitForCompletion();

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            await warningGroup.DOFade(0, FADE_OUT).AsyncWaitForCompletion();

            //Logo
            await PlayLogoAnimation();

            //await logoGroup.DOFade(1, 0.8f).AsyncWaitForCompletion();

            await UniTask.Delay(TimeSpan.FromSeconds(0.7f));

            //await logoGroup.DOFade(0, FADE_OUT).AsyncWaitForCompletion();

            //Tap Text
            StartTapAnimation();

            await WaitInput();

            await ExitAnimation();

            EnterMain();
        }

        private void InitUI()
        {
            warningGroup.alpha = 0;
            logoGroup.alpha = 0;
            startGroup.alpha = 0;

            startObject.SetActive(false);

            if (logoTransform != null)
                logoTransform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 로고 바운스 애니메이션
        /// </summary>
        private async UniTask PlayLogoAnimation()
        {
            logoGroup.alpha = 1;

            logoTransform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                logoTransform
                .DOScale(1.2f, 0.6f)
                .SetEase(Ease.OutBounce)
            );

            seq.Append(
                logoTransform
                .DOScale(1f, 0.2f)
            );

            await seq.AsyncWaitForCompletion();
        }

        /// <summary>
        /// Tap 텍스트 애니메이션
        /// </summary>
        private void StartTapAnimation()
        {
            startObject.SetActive(true);

            startGroup.alpha = 1;

            // 왼쪽에서 슬라이드 등장
            startTextTransform.anchoredPosition =
                new Vector2(-800, startTextTransform.anchoredPosition.y);

            startTextTransform
                .DOAnchorPosX(0, 0.8f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    PlayIdleAnimation();
                });
        }

        /// <summary>
        /// Tap 텍스트 Idle 애니메이션
        /// </summary>
        private void PlayIdleAnimation()
        {
            startGroup
                .DOFade(0.3f, 0.7f)
                .SetLoops(-1, LoopType.Yoyo);

            startTextTransform
                .DORotate(new Vector3(0, 0, 3), 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        //private void StartBlinkText()
        //{
        //    startObject.SetActive(true);

        //    startGroup
        //        .DOFade(1f, 0.8f)
        //        .SetLoops(-1, LoopType.Yoyo)
        //        .SetLink(startObject);
        //}

        private async UniTask ExitAnimation()
        {
            Sequence seq = DOTween.Sequence();

            seq.Join(
                logoGroup
                .DOFade(0, FADE_OUT)
            );

            seq.Join(
                startGroup
                .DOFade(0, FADE_OUT)
            );

            // 살짝 축소 연출 추가 (있으면 훨씬 자연스러움)
            if (logoTransform != null)
            {
                seq.Join(
                    logoTransform
                    .DOScale(0.8f, FADE_OUT)
                    .SetEase(Ease.InBack)
                );
            }

            await seq.AsyncWaitForCompletion();
        }

        private async UniTask WaitInput()
        {
            await UniTask.WaitUntil(() =>
                Touchscreen.current?.primaryTouch.press.isPressed == true ||
                Pointer.current?.press.isPressed == true ||
                Keyboard.current?.anyKey.isPressed == true
            );
        }

        private void EnterMain()
        {
            if (_isTransitioning) return;

            _isTransitioning = true;

            GameManager.Instance.EnterGameScene("Main").Forget();
        }
    }
}