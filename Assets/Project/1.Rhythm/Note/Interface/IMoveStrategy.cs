using UnityEngine;

namespace Project.Rhythm.Note.Interface
{
    /// <summary>
    /// 이동 전략
    /// </summary>
    public interface IMoveStrategy
    {
        void UpdateMove(Transform target, float progress);
    }
}
