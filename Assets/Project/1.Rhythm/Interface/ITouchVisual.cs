using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;

namespace Project.Rhythm.Interface
{
    /// <summary>
    /// 플레이어 입력(터치, 슬라이드 등)에 반응하는 공통 인터페이스
    /// </summary>
    public interface ITouchVisual
    {
        void PlayAction(PatternType type);
        void StopHoldAction();

        void PlayAction(JudgeResult result);

        void UpdateVisual(float progress);
    }
}
