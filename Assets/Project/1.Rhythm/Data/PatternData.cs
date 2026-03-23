using Project.Rhythm.Data.Struct;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Data
{
    public enum StageThemeType
    {
        Stage1, // 슬라이드
        Stage2, // 탭
        Stage3  // 홀드
    }
    /// <summary>
    /// 작은 단위인 액션을 넣은 패턴 묶음
    /// </summary>
    [CreateAssetMenu(fileName = "NewPattern", menuName = "Project/Rhythm/Pattern")]
    public class PatternData : ScriptableObject
    {
        public StageThemeType theme;
        public List<RhythmAction> actions = new();
        public float length;
    }
}
