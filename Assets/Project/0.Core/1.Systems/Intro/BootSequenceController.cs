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
            return warningGroup != null && logoGroup != null && startGroup != null && startObject != null;
        }

        private async UniTask RunIntro()
        {
            await GameManager.Instance.Initialize();

            InitUI();

            //Warning
            if (warningGroup != null)
            {
                await warningGroup.DOFade(1, FADE_IN).AsyncWaitForCompletion();
                await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
                await warningGroup.DOFade(0, FADE_OUT).AsyncWaitForCompletion();
            }

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
            if (warningGroup != null) warningGroup.alpha = 0;
            if (logoGroup != null) logoGroup.alpha = 0;
            if (startGroup != null) startGroup.alpha = 0;

            if (startObject != null) startObject.SetActive(false);

            if (logoTransform != null)
                logoTransform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 로고 바운스 애니메이션
        /// </summary>
        private async UniTask PlayLogoAnimation()
        {
            if (logoGroup == null || logoTransform == null) return;

            logoGroup.alpha = 1;
            logoTransform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(logoTransform.DOScale(1.2f, 0.6f).SetEase(Ease.OutBounce));
            seq.Append(logoTransform.DOScale(1f, 0.2f));

            await seq.AsyncWaitForCompletion();
        }

        /// <summary>
        /// Tap 텍스트 애니메이션
        /// </summary>
        private void StartTapAnimation()
        {
            if (startObject == null || startGroup == null || startTextTransform == null) return;

            startObject.SetActive(true);
            startGroup.alpha = 1;

            startTextTransform.anchoredPosition = new Vector2(-800, startTextTransform.anchoredPosition.y);

            startTextTransform
                .DOAnchorPosX(0, 0.8f)
                .SetEase(Ease.OutBack)
                .OnComplete(PlayIdleAnimation);
        }

        /// <summary>
        /// Tap 텍스트 Idle 애니메이션
        /// </summary>
        private void PlayIdleAnimation()
        {
            if (startGroup == null || startTextTransform == null) return;

            startGroup.DOFade(0.3f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            startTextTransform.DORotate(new Vector3(0, 0, 3), 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(gameObject);
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
            if (logoGroup != null) seq.Join(logoGroup.DOFade(0, FADE_OUT));
            if (startGroup != null) seq.Join(startGroup.DOFade(0, FADE_OUT));
            if (logoTransform != null) seq.Join(logoTransform.DOScale(0.8f, FADE_OUT).SetEase(Ease.InBack));

            await seq.AsyncWaitForCompletion();
        }

        private async UniTask WaitInput()
        {
            await UniTask.WaitUntil(() =>
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) ||
                (Pointer.current != null && Pointer.current.press.isPressed) ||
                (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
            );
        }

        private void EnterMain()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            //DOTween.KillAll();
            DOTween.Kill(gameObject);

            GameManager.Instance.EnterGameScene("Main").Forget(); 
            //GameManager.Instance.EnterGameScene("TestCore1").Forget();
        }
    }
}