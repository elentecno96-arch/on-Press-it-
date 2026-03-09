using Project.Rhythm.Data;
using UnityEngine;

namespace Project.Rhythm.Note.Interface
{
    /// <summary>
    /// 액션 전략
    /// </summary>
    public interface IActionStrategy
    {
        void Execute(TouchEventVisual visual, PatternType input);
    }
}
