using Project.Rhythm.Data;

namespace Project.Rhythm.Note.Interface
{
    /// <summary>
    /// 노트 상태
    /// </summary>
    public interface INoteState
    {
        void Enter();                               // 상태 진입 시 초기화
        void Update(float time);                    // 매 프레임 로직
        void HandleInput(PatternType input);        // 플레이어 입력 처리
        void Exit();                                // 상태 전환 시 정리
    }
}
