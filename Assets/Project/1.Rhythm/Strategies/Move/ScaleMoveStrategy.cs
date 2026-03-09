using Project.Rhythm.Note.Interface;
using UnityEngine;

namespace Project.Rhythm.Strategies
{
    /// <summary>
    /// 크기 변화 이동 전략
    /// </summary>
    public class ScaleMoveStrategy : IMoveStrategy
    {
        public void UpdateMove(Transform target, float progress)
        {
            float scale = Mathf.Lerp(0.2f, 1.5f, progress);

            target.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
