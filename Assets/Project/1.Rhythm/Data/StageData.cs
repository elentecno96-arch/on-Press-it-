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
        [HideInInspector] public bool isClear;
        [HideInInspector] public int bestScore;
        //외부에서 stageData.isClear를 호출하면 PlayerManager의 데이터를 즉시 확인

        public int BestScore => PlayerManager.Instance != null ? PlayerManager.Instance.GetBestScore(stageIndex) : 0;

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

        // 클리어 이벤트를 정의합니다. (인덱스와 점수를 전달)
        public static event System.Action<int> OnStagePlayCompleted;
        public void SetClear(int score)
        {
            // (float)score는 PlayerManager가 float 점수를 받기 때문입니다.
            OnStagePlayCompleted?.Invoke(this.stageIndex);
            Debug.Log($"[StageData] {stageName} 플레이 완료 이벤트 발송 (Index: {stageIndex})");
        }
    }
}
