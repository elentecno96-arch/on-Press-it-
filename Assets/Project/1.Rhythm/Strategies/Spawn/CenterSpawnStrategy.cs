using Project.Rhythm.Note.Interface;
using UnityEngine;

namespace Project.Rhythm.Strategies
{
    /// <summary>
    /// 중앙 스폰 전략
    /// </summary>
    public class CenterSpawnStrategy : ISpawnStrategy
    {
        public void SetPosition(Transform target)
        {
            if (target is RectTransform rect)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                target.localPosition = Vector3.zero;
            }
            target.localScale = Vector3.zero;

            target.localScale = Vector3.one;
        }
    }
}
