using Project.Rhythm.Data.Struct;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Data
{
    /// <summary>
    /// 스테이지 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewStage", menuName = "Project/Rhythm/Stage")]
    public class StageData : ScriptableObject
    {
        [Header("Status")]
        public int stageIndex; // 1, 2, 3... 순서대로 기입
        public bool isClear;   // 클리어 여부

        [Header("Audio")]
        public string stageName;
        public AudioClip masterTrack;
        public float bpm;      

        [Header("Theme Resources")]
        public List<ThemeResource> themeResources;

        public float perfectWindow = 0.12f;
        public float greatWindow = 0.21f;
        public float goodWindow = 0.27f;
        public float missWindow = 0.34f;

        [Header("Timing")]
        public float playStartTime;
        public float endPosition;

        [Header("Actions (Runtime)")]
        public List<RhythmAction> actions = new();

        [Header("Theme Events (Runtime)")]
        public List<ThemeEvent> themeEvents = new();

        [Header("Patterns (Editor Only)")]
        public List<PatternData> patterns = new();

        /// [추가] 이 스테이지가 클리어되었을 때 호출하여 상태를 변경합니다.
        /// public void SetClear(bool value)
        public void SetClear(bool value)
        {
            isClear = value;
            Debug.Log($"{stageName} 클리어 상태 변경: {value}");
        }
    }
}
