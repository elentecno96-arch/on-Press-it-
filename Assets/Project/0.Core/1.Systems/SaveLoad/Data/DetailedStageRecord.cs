using System.Collections.Generic;

namespace Project.Core.Systems.SaveLoad.Data
{
    /// <summary>
    /// 스테이지 별 상세 기록 래퍼 클래스
    /// </summary>
    [System.Serializable]
    public class DetailedStageRecord // 스테이지별 상세 기록 래퍼
    {
        public int stageIndex;
        public List<ScoreRecord> records = new();
    }
}
