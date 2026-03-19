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
        [Header("Audio")]
        public string stageName;
        public AudioClip masterTrack;
        public float bpm;

        [Header("Prefabs (Visual)")]    
        public GameObject backgroundPrefab;     // 배경용 프리팹 (가장 뒤)
        public GameObject playerPrefab;         // 플레이어 비주얼 프리팹 (가장 앞)
        public GameObject notePrefab;           // 노트 프리팹

        public float perfectWindow = 0.12f;
        public float greatWindow = 0.21f;
        public float goodWindow = 0.27f;
        public float missWindow = 0.34f;

        [Header("Timing")]
        public float playStartTime;       // 곡 시작 오프셋
        public float endPosition;         // 곡 종료 시점 (StageTime 기준)

        [Header("Patterns")]
        public List<RhythmAction> actions = new();
    }
}
