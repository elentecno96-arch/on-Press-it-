using Project.Rhythm.Data;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;

namespace Project.Rhythm.Presentation
{
    /// <summary>
    /// 게임 씬의 스테이지 비주얼 중재자.
    /// StageData를 기반으로 배경, 플레이어 연출, 노트를 생성하고 관리합니다.
    /// </summary>
    public class StagePresenter : MonoBehaviour
    {
        [SerializeField] private Transform backgroundRoot; // 배경이 생성될 부모
        [SerializeField] private Transform noteRoot;       // 노트가 생성될 부모
        [SerializeField] private Transform touchRoot;      // 플레이어 연출(손 등) 부모

        private StageData _stageData;
        private ITouchVisual _playerTouchVisual;           // 플레이어 피드백용

        public void Initialize(StageData data)
        {
            _stageData = data;

            if (!ValidateData()) return;

            ClearPreviousStage();

            SpawnBackground();
            SpawnPlayerVisual();
        }

        private bool ValidateData()
        {
            if (_stageData == null) { Debug.LogError("StageData가 할당되지 않았습니다."); return false; }
            if (_stageData.backgroundPrefab == null) { Debug.LogError("BackgroundPrefab이 없습니다."); return false; }
            if (_stageData.playerPrefab == null) { Debug.LogError("PlayerPrefab이 없습니다."); return false; }
            if (_stageData.notePrefab == null) { Debug.LogError("NotePrefab이 없습니다."); return false; }
            return true;
        }

        private void ClearPreviousStage()
        {
            foreach (Transform child in backgroundRoot) Destroy(child.gameObject);
            foreach (Transform child in touchRoot) Destroy(child.gameObject);
            foreach (Transform child in noteRoot) Destroy(child.gameObject);
        }

        private void SpawnBackground()
        {
            Instantiate(_stageData.backgroundPrefab, backgroundRoot);
        }

        private void SpawnPlayerVisual()
        {
            GameObject playerObj = Instantiate(_stageData.playerPrefab, touchRoot);

            _playerTouchVisual = playerObj.GetComponentInChildren<ITouchVisual>();

            if (_playerTouchVisual == null)
            {
                Debug.LogWarning("플레이어 프리팹에서 ITouchVisual을 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// StageManager에서 플레이어 입력 피드백을 보내기 위해 호출
        /// </summary>
        public ITouchVisual GetTouchVisual()
        {
            return _playerTouchVisual;
        }

        /// <summary>
        /// StageManager에서 새로운 노트를 생성할 때 호출
        /// </summary>
        public GameObject SpawnNote()
        {
            if (_stageData.notePrefab == null) return null;

            GameObject noteObj = Instantiate(_stageData.notePrefab, noteRoot);
            return noteObj;
        }

        public void ShowJudgeEffect(JudgeResult result)
        {
            //판정 연출 용
            Debug.Log($"<color=white>[Visual Effect]</color> {result}");
        }
    }
}