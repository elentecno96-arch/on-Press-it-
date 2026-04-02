using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Project.Rhythm.Judgement;

namespace Project.Core.Ui.StageUi.View
{
    public class PlayUiView : MonoBehaviour
    {
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Judgement UI")]
        [SerializeField] private CanvasGroup judgeCanvasGroup; // 투명도 조절용
        [SerializeField] private Image judgeImage;
        [SerializeField] private Sprite[] judgeSprites;

        [Header("Tween Settings")]
        [SerializeField] private float animDuration = 0.2f;
        [SerializeField] private float startScale = 0.5f;
        [SerializeField] private float endScale = 1.2f;

        private Sequence _judgeSequence;
        private int _lastDisplayedScore = 0;

        private void Awake()
        {
            if (judgeCanvasGroup != null)
            {
                judgeCanvasGroup.alpha = 0f;
                judgeCanvasGroup.transform.localScale = Vector3.one * startScale;
            }
            UpdateScore(0);
        }

        public void UpdateScore(float score)
        {
            int targetScore = Mathf.RoundToInt(score);

            DOTween.To(() => _lastDisplayedScore, x => {
                _lastDisplayedScore = x;
                scoreText.text = _lastDisplayedScore.ToString("N0");
            }, targetScore, 0.25f).SetEase(Ease.OutQuad);
        }

        public void ShowJudgement(JudgeResult result)
        {
            if (judgeImage == null || judgeCanvasGroup == null) return;

            int index = (int)result;
            if (index >= judgeSprites.Length) return;
            judgeImage.sprite = judgeSprites[index];

            _judgeSequence?.Kill(true);

            _judgeSequence = DOTween.Sequence()
                .OnStart(() => {
                    judgeCanvasGroup.alpha = 1f;
                    judgeCanvasGroup.transform.localScale = Vector3.one * startScale;
                })

                .Append(judgeCanvasGroup.transform.DOScale(endScale, animDuration).SetEase(Ease.OutBack))

                .AppendInterval(0.05f)
                .Append(judgeCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.InQuad))
                .Join(judgeCanvasGroup.transform.DOScale(endScale * 1.1f, 0.15f))
                .OnComplete(() => {
                    judgeCanvasGroup.alpha = 0f;
                });
        }

        private void OnDestroy()
        {
            _judgeSequence?.Kill();
        }
    }
}