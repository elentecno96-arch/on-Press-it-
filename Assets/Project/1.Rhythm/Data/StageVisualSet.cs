using UnityEngine;

namespace Project.Rhythm.Data
{
    /// <summary>
    /// 터치 비주얼 및 노트 프리펩 SO
    /// </summary>
    [CreateAssetMenu(fileName = "StageVisualSet", menuName = "Project/Rhythm/StageVisualSet")]
    public class StageVisualSet : ScriptableObject
    {
        public GameObject notePrefab;
        public GameObject touchVisualPrefab;
    }
}