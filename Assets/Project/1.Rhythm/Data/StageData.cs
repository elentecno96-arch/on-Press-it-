using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Data
{
    /// <summary>
    /// 스테이지 SO 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewStage", menuName = "Project/Rhythm/Stage")]
    public class StageData : ScriptableObject
    {
        public AudioClip masterTrack;                   // 메인 노래
        public float bpm;

        public GameObject backgroundPrefab;             // 배경
        public StageVisualSet visualSet;

        public float playStartTime;                     // 이벤트가 시작하는 위치
        public float endPosition;                       // 스테이지가 끝나는 위치

        public List<PatternData> patterns;              // 스테이지에 배치될 패턴
    }
}
