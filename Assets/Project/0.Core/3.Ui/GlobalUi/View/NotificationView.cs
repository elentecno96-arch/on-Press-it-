using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Managers;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Project.Core.Ui.GlobalUi.View
{
    public class NotificationView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private RectTransform rectTransform;

        [SerializeField] private AudioClip showSfx;
        [SerializeField] private AudioClip hideSfx;

        private Vector2 _originPos;
        private const float ANIM_TIME = 0.4f;
        private const float OFFSET_Y = 50f; 

        public void Init()
        {
            if (rectTransform != null) _originPos = rectTransform.anchoredPosition;
            if (canvasGroup != null) canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        public async UniTask ShowMessage(string message, float duration, CancellationToken token)
        {
            rectTransform.DOKill();
            canvasGroup.DOKill();

            messageText.text = message;

            rectTransform.anchoredPosition = new Vector2(_originPos.x, _originPos.y + OFFSET_Y);
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);

            try
            {
                if (showSfx != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(showSfx);

                await DOTween.Sequence()
                    .Join(rectTransform.DOAnchorPosY(_originPos.y, ANIM_TIME).SetEase(Ease.OutCubic))
                    .Join(canvasGroup.DOFade(1f, ANIM_TIME))
                    .WithCancellation(token);

                await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);

                if (hideSfx != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(hideSfx);

                await DOTween.Sequence()
                    .Join(rectTransform.DOAnchorPosY(_originPos.y - OFFSET_Y, ANIM_TIME).SetEase(Ease.InCubic))
                    .Join(canvasGroup.DOFade(0f, ANIM_TIME))
                    .WithCancellation(token);

                gameObject.SetActive(false);
            }
            catch (System.OperationCanceledException)
            {
                canvasGroup.alpha = 0;
                rectTransform.anchoredPosition = _originPos;
                gameObject.SetActive(false);
            }
        }
    }
}