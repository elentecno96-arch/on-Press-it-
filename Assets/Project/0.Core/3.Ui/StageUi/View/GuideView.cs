using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Project.Rhythm.Data.Enum;

namespace Project.Core.Ui.StageUi.View
{
    /// <summary>
    /// 플레이 시작 가이드 뷰
    /// </summary>
    public class GuideView : MonoBehaviour
    {
        [SerializeField] private Image guideImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip appearSfx;

        [Header("Animation Settings")]
        [SerializeField] private float frameRate = 0.1f;

        // 각 패턴별 프레임 배열
        [SerializeField] private Sprite[] tapFrames;
        [SerializeField] private Sprite[] slideFrames;
        [SerializeField] private Sprite[] holdFrames;

        private Coroutine _animationCoroutine;
        private Sprite[] _currentFrames;

        public void Show(PatternType type)
        {
            _currentFrames = type switch
            {
                PatternType.Tap => tapFrames,
                PatternType.Slide => slideFrames,
                PatternType.Hold => holdFrames,
                _ => tapFrames
            };

            if (_currentFrames == null || _currentFrames.Length == 0) return;

            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1f, 0.4f);

            if (sfxSource != null && appearSfx != null)
                sfxSource.PlayOneShot(appearSfx);

            StopAnimation();
            _animationCoroutine = StartCoroutine(PlayAnimationRoutine());
        }

        private IEnumerator PlayAnimationRoutine()
        {
            int index = 0;
            while (true)
            {
                guideImage.sprite = _currentFrames[index];
                index = (index + 1) % _currentFrames.Length;
                yield return new WaitForSeconds(frameRate);
            }
        }

        public void Hide()
        {
            canvasGroup.DOFade(0f, 0.4f).OnComplete(() =>
            {
                StopAnimation();
                gameObject.SetActive(false);
            });
        }

        private void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }

        public void SetupDefault()
        {
            StopAnimation();
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopAnimation();
        }
    }
}
