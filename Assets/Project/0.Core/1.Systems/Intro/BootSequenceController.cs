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

        [SerializeField] private CanvasGroup warningGroup;
        [SerializeField] private CanvasGroup logoGroup;
        [SerializeField] private CanvasGroup startGroup;

        [SerializeField] private GameObject startObject;

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

            await warningGroup.DOFade(1, FADE_IN).AsyncWaitForCompletion();

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            await warningGroup.DOFade(0, FADE_OUT).AsyncWaitForCompletion();

            await logoGroup.DOFade(1, 0.8f).AsyncWaitForCompletion();

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            await logoGroup.DOFade(0, FADE_OUT).AsyncWaitForCompletion();

            StartBlinkText();

            await WaitInput();

            EnterMain();
        }

        private void InitUI()
        {
            warningGroup.alpha = 0;
            logoGroup.alpha = 0;
            startGroup.alpha = 0;
            startObject.SetActive(false);
        }

        private void StartBlinkText()
        {
            startObject.SetActive(true);

            startGroup
                .DOFade(1f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(startObject);
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

            GameManager.Instance.EnterGameScene("TestCore1").Forget(); //테스트 씬
            //GameManager.Instance.EnterGameScene("Main").Forget();
        }
    }
}