using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using Project.Data.Stage.STAGE3;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Presentation
{
    /// <summary>
    /// 게임 씬의 스테이지 비주얼 중재자.
    /// StageData를 기반으로 배경, 플레이어 연출, 노트를 생성하고 관리합니다.
    /// </summary>
    public class StagePresenter : MonoBehaviour
    {
        [SerializeField] private StageView stageView;
        [SerializeField] private GameView inGameView;
        [SerializeField] private ResultView resultView;

        private Stage3Environment _environment;

        private readonly Dictionary<int, Note.Note> _fixedNoteMap = new();

        private StageData _stageData;
        private ITouchVisual _playerTouchVisual;           // 플레이어 피드백용

        public void Initialize(StageData data)
        {
            _stageData = data;
            if (stageView == null) return;

            stageView.Clear();

            GameObject bgObj = stageView.CreateBackground(_stageData.backgroundPrefab);
            _environment = bgObj.GetComponent<Stage3Environment>();

            if (_environment != null)
            {
                _environment.SetBpm(_stageData.bpm);
            }

            GameObject playerObj = stageView.CreatePlayer(_stageData.playerPrefab);
            _playerTouchVisual = playerObj.GetComponentInChildren<ITouchVisual>();

            InitializeUI();

            CachePersistentNotes(bgObj);
        }

        private void InitializeUI()
        {
            Debug.Log("<color=cyan>[Presenter]</color> UI 초기화 시작");

            if (inGameView != null)
            {
                inGameView.gameObject.SetActive(true); 
                inGameView.SetStageName(_stageData.stageName);
                inGameView.UpdateProgress(0f);
            }
            else
            {
                Debug.LogError("[Presenter] inGameView가 할당되지 않았습니다!");
            }

            if (resultView != null)
            {
                resultView.gameObject.SetActive(false); // 결과창은 처음에 꺼야 함
            }
        }

        /// <summary>
        /// StageManager의 Update에서 호출되어 UI를 실시간으로 갱신합니다.
        /// </summary>
        public void UpdateUI(float currentTime)
        {
            if (inGameView == null || _stageData == null) return;

            // 진행도(Progress Bar) 업데이트
            float progress = Mathf.Clamp01(currentTime / _stageData.endPosition);
            inGameView.UpdateProgress(progress);
        }

        /// <summary>
        /// 곡이 끝났을 때 StageManager로부터 판정 데이터를 받아 결과창을 출력
        /// </summary>
        public void ShowResult(int p, int gr, int go, int m)
        {
            // 인게임 UI 숨기기
            if (inGameView != null) inGameView.gameObject.SetActive(false);

            // 결과창 표시
            if (resultView != null)
            {
                resultView.DisplayResult(p, gr, go, m);
            }
        }

        private void CachePersistentNotes(GameObject bgRoot)
        {
            _fixedNoteMap.Clear();
            var notes = bgRoot.GetComponentsInChildren<Note.Note>(true);

            foreach (var note in notes)
            {
                if (note.IsPersistent && !string.IsNullOrEmpty(note.NoteID))
                {
                    int key = note.NoteID.GetHashCode();
                    if (!_fixedNoteMap.ContainsKey(key))
                    {
                        _fixedNoteMap.Add(key, note);
                        Debug.Log($"<color=green>[Presenter]</color> 고정 노트 연결: {note.NoteID}");
                    }
                }
            }
        }

        public Note.Note GetFixedNote(string id) =>
            _fixedNoteMap.TryGetValue(id.GetHashCode(), out var note) ? note : null;

        public ITouchVisual GetTouchVisual() => _playerTouchVisual;

        public GameObject SpawnNote() => stageView.CreateNote(_stageData.notePrefab);

        public void ShowJudgeEffect(JudgeResult result)
        {
            Debug.Log($"<color=white>[Visual Effect]</color> {result}");
        }

        public void StartCountdown(float targetBeat)
        {
            _environment?.StartCountdown(targetBeat);
        }

        public void StopCountdown() => _environment?.StopCountdown();
    }
}