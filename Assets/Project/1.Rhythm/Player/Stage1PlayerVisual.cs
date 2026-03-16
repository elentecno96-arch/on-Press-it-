using DG.Tweening;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using UnityEngine;

namespace Project.Rhythm.Player
{
    /// <summary>
    /// 1스테이지 플레이어 비주얼
    /// </summary>
    public class Stage1PlayerVisual : BaseRhythmVisual
    {
        [SerializeField] private AudioClip slideSfx;

        public override void PlayAction(PatternType type)
        {
            if (type == PatternType.Slide)
            {
                PlaySfx(slideSfx != null ? slideSfx : actionSfx);

                SetAnimation(actionFrames, false);

                transform.DOComplete();
                transform.DOPunchPosition(new Vector3(50f, 0, 0), 0.15f);
            }
        }

        public override void PlayAction(JudgeResult result)
        {
            _isHolding = false;
            _isJudged = false; 

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                transform.DOComplete();
                transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);
            }
            else
            {
                PlaySfx(missSfx);
                transform.DOComplete();
                transform.DOShakePosition(0.2f, 10f);
            }
        }

        public override void UpdateVisual(float progress) { }
    }
}