using DG.Tweening;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using UnityEngine;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 1스테이지 선반 노트
    /// </summary>
    public class ShelfVisual : BaseRhythmVisual
    {
        public override void UpdateVisual(float progress)
        {
            float targetScale;

            if (progress <= 1.0f)
            {
                targetScale = Mathf.Lerp(0.05f, 1.0f, progress);
            }
            else
            {
                float overshootProgress = (progress - 1.0f) / 1.0f;
                targetScale = Mathf.Lerp(1.0f, 2.5f, overshootProgress);
                if (progress > 1.5f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (progress - 1.5f) / 0.5f);
                    targetImage.color = new Color(1, 1, 1, alpha);
                }
            }

            transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        public override void PlayAction(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                SetAnimation(successFrames,successFrameRate, true);

                transform.DOComplete();
                transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
            }
            else
            {
                PlaySfx(missSfx);
                SetAnimation(missFrames,missFrameRate, true);

                transform.DOComplete();
                transform.DOShakePosition(0.3f, 10f);
                targetImage.DOFade(0, 0.3f).SetDelay(0.2f);
            }
        }

        public override void PlayAction(Project.Rhythm.Data.Enum.PatternType type)
        {
            if (type == Project.Rhythm.Data.Enum.PatternType.Tap || type == Project.Rhythm.Data.Enum.PatternType.Slide)
            {
                PlaySfx(actionSfx);
            }
        }

        public override void ResetVisual()
        {
            base.ResetVisual();
            if (targetImage != null) targetImage.color = Color.white;
            transform.localScale = Vector3.one;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            transform.DOKill();
            targetImage.DOKill();

            _isJudged = false;

            if (targetImage != null) targetImage.color = Color.white;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }
    }
}