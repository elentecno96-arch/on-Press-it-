using Project.Core.Managers;
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
        public bool skipGuide;
        //외부에서 stageData.isClear를 호출하면 PlayerManager의 데이터를 즉시 확인
        public bool isClear => PlayerManager.Instance != null && PlayerManager.Instance.IsStageCleared(stageIndex); //clear 여부는 PlayerManager의 데이터를 참조하여 반환

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

        // 이 메서드는 남겨두되, 내부에 로직만 넣습니다.
        public void SetClear(bool value)
        {
            // 이 필드는 런타임 확인용 로그입니다.
            Debug.Log($"{stageName} 클리어 상태 변경 시도: {value}");
        }
    }
}
