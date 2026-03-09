using Project.Rhythm.Data;

namespace Project.Rhythm.Interface
{
    /// <summary>
    /// 플레이어 입력(터치, 슬라이드 등)에 반응하는 공통 인터페이스
    /// </summary>
    public interface ITouchVisual
    {
        /// <summary>
        /// 입력하는 형태에 따라 반응 연출
        /// </summary>
        /// <param name="type"></param>
        void PlayAction(PatternType type);
    }
}
