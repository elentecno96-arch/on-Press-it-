using Project.Rhythm.Data;
using Project.Rhythm.Data.Struct;
using System.Collections.Generic;

namespace Project.Editor.TestEditor
{
    /// <summary>
    /// JSON 변환을 위한 헬퍼 클래스
    /// </summary>
    [System.Serializable]
    public class StageSaveData
    {
        public string stageName;
        public float bpm;
        public List<RhythmAction> actions;
        public List<ThemeEvent> themeEvents;
    }
}
