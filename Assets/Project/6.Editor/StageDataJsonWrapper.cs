using Project.Rhythm.Data;
using Project.Rhythm.Data.Struct;
using System.Collections.Generic;

namespace Project.Editor
{
    /// <summary>
    /// json으로 저장/불러오기 위한 래퍼 클래스
    /// </summary>
    [System.Serializable]
    public class StageDataJsonWrapper
    {
        public List<RhythmAction> actions;
        public List<ThemeEvent> themeEvents;
    }
}
