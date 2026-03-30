using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;

namespace Project.Data.Stage.STAGE2
{
    /// <summary>
    /// 스테이지 2의 배경 비주얼
    /// </summary>
    public class S2Background : BaseRhythmVisual
    {
        protected override void Awake()
        {
            // 부모의 Awake에서 targetImage 할당 및 초기 idle 애니메이션 세팅이 수행됩니다.
            base.Awake();
        }

        /// <summary>
        /// 카운트 처리
        /// </summary>
        /// <param name="targetBeat"></param>
        public override void StartCountdown(float targetBeat)
        {
            //스테이지1에 카운트를 사용한다면 채워주세요
        }

        /// <summary>
        /// 타입에 따른 배경 연출용 메서드
        /// </summary>
        /// <param name="type"></param>
        public override void PlayAction(PatternType type)
        {
            // 배경이 특정 입력에 반응해야 한다면 여기서 SetAnimation 등을 호출합니다.
        }

        /// <summary>
        /// 판정 결과에 따른 배경 연출 용 메서드
        /// </summary>
        /// <param name="result"></param>
        public override void PlayAction(JudgeResult result)
        {
            // 판정(Perfect/Miss)에 따라 배경 리액션을 넣고 싶을 때 구현합니다.
        }
    }
}
