using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Data
{
    /// <summary>
    /// 패턴 타입 열거형
    /// </summary>
    public enum PatternType { None, Tap, Hold, Slide }

    /// <summary>
    /// 리듬 행동 이벤트
    /// </summary>
    [Serializable]
    public struct RhythmAction
    {
        public float beat;          // 박자 단위
        public PatternType type;
        public float duration;      // Hold 패턴에서 사용 예정

        public string spawnStrategyId;
        public string moveStrategyId;
        public string actionStrategyId;
    }

    /// <summary>
    /// 순수 패턴 리스트를 담은 SO 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewPattern", menuName = "Project/Rhythm/Pattern")]
    public class PatternData : ScriptableObject
    {
        public List<RhythmAction> actions;
    }
}
