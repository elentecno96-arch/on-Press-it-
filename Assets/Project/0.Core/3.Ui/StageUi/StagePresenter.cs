using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
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
        [SerializeField] private StageView view;

        private readonly Dictionary<int, Note.Note> _fixedNoteMap = new();

        private StageData _stageData;
        private ITouchVisual _playerTouchVisual;           // 플레이어 피드백용

        public void Initialize(StageData data)
        {
            _stageData = data;
            if (view == null) return;

            view.Clear();

            view.CreateBackground(_stageData.backgroundPrefab);
            GameObject playerObj = view.CreatePlayer(_stageData.playerPrefab);

            _playerTouchVisual = playerObj.GetComponentInChildren<ITouchVisual>();
            CachePersistentNotes();
        }

        private void CachePersistentNotes()
        {
            _fixedNoteMap.Clear();
            var notes = GetComponentsInChildren<Note.Note>(true);
            foreach (var note in notes)
            {
                if (note.IsPersistent && !string.IsNullOrEmpty(note.NoteID))
                {
                    _fixedNoteMap.TryAdd(note.NoteID.GetHashCode(), note);
                }
            }
        }

        public Note.Note GetFixedNote(string id) =>
            _fixedNoteMap.TryGetValue(id.GetHashCode(), out var note) ? note : null;

        /// <summary>
        /// StageManager에서 플레이어 입력 피드백을 보내기 위해 호출
        /// </summary>
        public ITouchVisual GetTouchVisual() => _playerTouchVisual;

        /// <summary>
        /// StageManager에서 새로운 노트를 생성할 때 호출
        /// </summary>
        public GameObject SpawnNote() => view.CreateNote(_stageData.notePrefab);

        public void ShowJudgeEffect(JudgeResult result)
        {
            //판정 연출 용
            Debug.Log($"<color=white>[Visual Effect]</color> {result}");
        }
    }
}