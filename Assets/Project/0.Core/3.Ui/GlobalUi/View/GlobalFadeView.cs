using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Core.Ui.GlobalUi.View
{
    /// <summary>
    /// 글로벌 페이드 전용 뷰
    /// </summary>
    public class GlobalFadeView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.4f;

        private Tween _fadeTween;

        public void Init()
        {
            fadeGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public async UniTask PlayFade(float targetAlpha)
        {
            _fadeTween?.Kill();

            if (targetAlpha > 0)
            {
                gameObject.SetActive(true);
            }

            _fadeTween = fadeGroup.DOFade(targetAlpha, fadeDuration);
            await _fadeTween.ToUniTask();

            if (targetAlpha <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
