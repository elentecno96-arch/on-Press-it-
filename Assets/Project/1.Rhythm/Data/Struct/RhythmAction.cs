using System;
using UnityEngine;
using Project.Rhythm.Data.Enum;

namespace Project.Rhythm.Data.Struct
{
    /// <summary>
    /// 리듬 입력 이벤트 (판정 전용 데이터)
    /// </summary>
    [Serializable]
    public struct RhythmAction
    {
        [Tooltip("박자 기준 위치")]
        public float beat;

        [Tooltip("입력 타입")]
        public PatternType type;

        [Tooltip("Hold 지속 시간")]
        public float duration;
    }
}
