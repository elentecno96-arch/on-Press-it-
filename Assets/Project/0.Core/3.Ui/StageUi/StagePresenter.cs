using Project.Rhythm.Data;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;

namespace Project.Rhythm.Presentation
{
    /// <summary>
    /// 게임 씬의 스테이지 중재자
    /// </summary>
    public class StagePresenter : MonoBehaviour
    {
        [SerializeField] private Transform backgroundRoot;
        [SerializeField] private Transform noteRoot;
        [SerializeField] private Transform touchRoot;

        private StageData _stageData;
        private ITouchVisual _touchVisual;

        private GameObject _lastSpawned;

        public void Initialize(StageData data)
        {
            _stageData = data;

            ValidateData();

            SpawnBackground();
            SpawnTouchVisual();
        }

        private void ValidateData()
        {
            if (_stageData == null)
                Debug.LogError("StageData is null");

            if (_stageData.visualSet == null)
                Debug.LogError("StageVisualSet is null");

            if (_stageData.visualSet.notePrefab == null)
                Debug.LogError("NotePrefab is not assigned");

            if (noteRoot == null)
                Debug.LogError("NoteRoot is not assigned");
        }

        private void SpawnBackground()
        {
            if (_stageData.backgroundPrefab == null)
                return;

            Instantiate(_stageData.backgroundPrefab, backgroundRoot);
        }

        private void SpawnTouchVisual()
        {
            if (_stageData.visualSet.touchVisualPrefab == null)
                return;

            GameObject obj = Instantiate(_stageData.visualSet.touchVisualPrefab, touchRoot);
            _touchVisual = obj.GetComponent<ITouchVisual>();

            if (_touchVisual == null)
                Debug.LogError("TouchVisual prefab does not implement ITouchVisual");
        }

        public ITouchVisual GetTouchVisual()
        {
            return _touchVisual;
        }

        public GameObject SpawnNote()
        {
            if (_stageData?.visualSet?.notePrefab == null)
                return null;

            GameObject obj = Instantiate(_stageData.visualSet.notePrefab, noteRoot);

            obj.name = "TEST_NOTE";

            Debug.Log("Spawned: " + obj.name);

            _lastSpawned = obj;

            Invoke(nameof(CheckAlive), 0.5f);

            return obj;
        }

        private void CheckAlive()
        {
            if (_lastSpawned == null)
                Debug.Log("NOTE DESTROYED");
            else
                Debug.Log("NOTE STILL ALIVE: " + _lastSpawned.name);
        }

        public void ShowJudgeEffect(JudgeResult result)
        {
            // TODO: 판정 이펙트 표시
        }
    }
}