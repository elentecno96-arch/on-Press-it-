namespace Project.Core.Systems.SaveLoad.Data
{
    /// <summary>
    /// 스테이지 별 최고 점수 및 상세 기록 래퍼 클래스
    /// </summary>
    [System.Serializable]
    public class StageSaveData
    {
        public int stageIndex;
        public float bestScore;
    }
}
