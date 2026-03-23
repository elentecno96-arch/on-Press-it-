namespace Project.Rhythm.Data
{
    /// <summary>
    /// 테마 스위칭 이벤트
    /// </summary>
    [System.Serializable]
    public struct ThemeEvent
    {
        public float beat;
        public StageThemeType theme;
    }
}
