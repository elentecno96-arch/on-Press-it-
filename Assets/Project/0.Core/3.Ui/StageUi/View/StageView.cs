using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Ui.StageUi.View
{
    /// <summary>
    /// 스테이지의 ui를 담당하는 뷰
    /// </summary>
    public class StageView : MonoBehaviour
    {
        [SerializeField] private Transform backgroundRoot;
        [SerializeField] private Transform noteRoot;
        [SerializeField] private Transform touchRoot;

        //private readonly List<StageLayer> _layers = new();
        public Transform NoteRoot => noteRoot;

        public void Clear()
        {
            foreach (Transform t in new[] { backgroundRoot, touchRoot, noteRoot })
            {
                if (t == null) continue;
                foreach (Transform child in t) Destroy(child.gameObject);
            }
        }

        public GameObject CreateBackground(GameObject prefab) => Instantiate(prefab, backgroundRoot);

        public GameObject CreatePlayer(GameObject prefab) => Instantiate(prefab, touchRoot);

        public GameObject CreateNote(GameObject prefab) => Instantiate(prefab, noteRoot);
    }
}
