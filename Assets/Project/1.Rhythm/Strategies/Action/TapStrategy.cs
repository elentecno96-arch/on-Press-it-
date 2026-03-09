using Project.Rhythm.Data;
using Project.Rhythm.Note.Interface;
using UnityEngine;

namespace Project.Rhythm.Strategies
{
    /// <summary>
    /// 탭 행동 전략
    /// </summary>
    public class TapStrategy : IActionStrategy
    {
        public void Execute(TouchEventVisual visual, PatternType input)
        {
            if (visual != null)
            {
                visual.PlayAction(input);
            }
        }
    }
}
