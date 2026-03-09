using UnityEngine;

namespace Project.Rhythm.Note.Interface
{
    /// <summary>
    /// 스폰 전략
    /// </summary>
    public interface ISpawnStrategy
    {
        void SetPosition(Transform target);
    }
}
