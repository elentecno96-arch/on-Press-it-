using Project.Rhythm.Data.Enum;
using System;
using UnityEngine;
using Project.Rhythm.Note;

namespace Project.Rhythm.Data.Struct
{
    //판정인지, 연출인지 선택 열거형
    public enum ActionRole { Hit, Signal }

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
        public ActionRole role;

        [Tooltip("Hold 지속 시간")]
        public float duration;

        [Tooltip("특정 노트를 지정하고 싶을 때 사용 (정적 노트 ID)")]
        public string targetID;
        [SerializeField] public NoteType noteType;
    }
}
