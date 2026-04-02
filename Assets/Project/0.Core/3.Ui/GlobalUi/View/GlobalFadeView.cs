using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading; // [필수 추가]
using UnityEngine;

namespace Project.Core.Ui.GlobalUi.View
{
    public class GlobalFadeView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.2f;

        private Tween _fadeTween;

        public void Init()
        {
            SetAlphaImmediate(0f);
        }

        public void SetAlphaImmediate(float alpha)
        {
            _fadeTween?.Kill();
            fadeGroup.alpha = alpha;
            gameObject.SetActive(alpha > 0);
        }

        public async UniTask PlayFade(float targetAlpha, float duration = -1f, CancellationToken token = default)
        {
            _fadeTween?.Kill();

            if (targetAlpha > 0)
            {
                gameObject.SetActive(true);
            }

            float finalDuration = (duration < 0) ? fadeDuration : duration;

            _fadeTween = fadeGroup.DOFade(targetAlpha, finalDuration);

            try
            {
                await _fadeTween.ToUniTask(cancellationToken: token);

                if (targetAlpha <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
            catch (System.OperationCanceledException)
            {
                _fadeTween?.Kill();
            }
        }
    }
}