using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using UnityEngine;

namespace Project.Data.Stage.STAGE2
{
    /// <summary>
    /// 스테이지 2의 배경 비주얼
    /// BaseRhythmVisual을 상속받아 판정 시 랜덤 리액션을 수행합니다.
    /// </summary>
    public class S2Background : BaseRhythmVisual
    {
        [Header("--- S2 Custom Settings ---")]
        [SerializeField] private float reactionDuration = 0.5f; 

        protected override void Awake()
        {
            base.Awake();
        }

        public override void StartCountdown(float targetBeat)
        {
            
        }

        public override void PlayAction(PatternType type)
        {
            
        }

        /// <summary>
        /// 판정 결과에 따른 배경 리액션
        /// </summary>
        public override void PlayAction(JudgeResult result)
        {
            if (result == JudgeResult.Miss || successFrames == null || successFrames.Length == 0) return;

            int randomIndex = Random.Range(0, successFrames.Length);
            Sprite selectedSprite = successFrames[randomIndex];

            Sprite[] fakeArray = new Sprite[] { selectedSprite, selectedSprite };

            SetAnimation(fakeArray, reactionDuration, false);

            PlaySfx(successSfx);
        }

        /// <summary>
        /// 단일 프레임 애니메이션이 끝났을 때(reactionDuration 경과 후) 호출됨
        /// </summary>
        protected override void OnAnimationComplete()
        {
            SetAnimation(idleFrames, idleFrameRate, true);
        }
    }
}