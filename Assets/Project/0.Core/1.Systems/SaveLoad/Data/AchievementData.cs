namespace Project.Core.Systems.SaveLoad.Data
{
    /// <summary>
    /// 플레이어 업적 데이터 래퍼 클래스
    /// </summary>
    [System.Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        public bool isUnlocked;
        public string unlockDate;
    }
}
