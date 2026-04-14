using DG.Tweening;
using Project.Rhythm.Judgement;
using Project.Rhythm.Data.Enum;
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
                float overshootProgress = (progress - 1.0f);
                targetScale = Mathf.Lerp(1.0f, 2.0f, overshootProgress);

                if (progress > 1.7f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (progress - 1.7f) / 0.3f);
                    targetImage.color = new Color(1, 1, 1, alpha);
                }
                else
                {
                    targetImage.color = Color.white;
                }
            }
            transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        public override void PlayAction(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            transform.DOComplete();

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                SetAnimation(successFrames, successFrameRate, false);

                transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            }
            else
            {
                PlaySfx(missSfx);
                SetAnimation(missFrames, missFrameRate, true);

                transform.DOShakePosition(0.3f, 10f);
                targetImage.DOFade(0, 0.3f).SetDelay(0.2f);
            }
        }

        public override void PlayAction(PatternType type)
        {
            if (type == PatternType.Tap || type == PatternType.Slide)
            {
                PlaySfx(actionSfx);
            }
        }

        public override void ResetVisual()
        {
            base.ResetVisual(); 

            if (targetImage != null)
            {
                targetImage.DOKill();
                targetImage.color = Color.white; 
            }

            transform.localScale = Vector3.one;
        }

        protected override void OnDisable()
        {
            transform.DOKill();
            targetImage.DOKill();
            base.OnDisable();
        }

        public override void StartCountdown(float targetBeat)
        {
            // 노트 비주얼이므로 배경 카운트다운 로직은 필요 없음
        }
    }
}