using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using UnityEngine;
using DG.Tweening;

namespace Project.Rhythm.Player
{

    /// <summary>
    /// 스테이지 2 플레이어 비주얼
    /// </summary>
    public class Stage2PlayerVisual : BaseRhythmVisual
    {
        [SerializeField] private AudioClip grabSfx;

        public override void PlayAction(PatternType type)
        {
            PlaySfx(grabSfx != null ? grabSfx : actionSfx);

            SetAnimation(actionFrames, false);

            transform.DOComplete();
            transform.DOPunchPosition(new Vector3(0, 50f, 0), 0.15f);
        }

        public override void PlayAction(JudgeResult result)
        {
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
                transform.DOShakePosition(0.2f, 15f);
            }
        }

        public override void UpdateVisual(float progress) { }
    }
}
