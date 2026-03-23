using UnityEngine;

namespace Project.Rhythm.Data
{
    /// <summary>
    /// 테마별로 데이터를 묶어 놓는 클래스
    /// </summary>
    [System.Serializable]
    public class ThemeResource
    {
        public StageThemeType theme;

        public GameObject backgroundPrefab;
        public GameObject playerPrefab;
        public GameObject notePrefab;
    }
}
