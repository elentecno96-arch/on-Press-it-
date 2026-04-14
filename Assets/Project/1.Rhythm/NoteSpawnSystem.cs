using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using Project.Rhythm.Pool;
using Project.Rhythm.Presentation;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm
{
    /// <summary>
    /// 노트 생성 담당 시스템
    /// 생성형 노트와 정적(미리 배치된) 노트를 통합 관리
    /// </summary>
    public class NoteSpawnSystem
    {
        private readonly StagePresenter _presenter;
        private readonly NotePool _runtimeNotePool;
        private readonly Dictionary<int, Note.Note> _fixedNoteMap = new();

        public NoteSpawnSystem(StagePresenter presenter)
        {
            _presenter = presenter;

            _runtimeNotePool = new NotePool(
                createFunc: () => _presenter.SpawnNote().GetComponent<Note.Note>()
            );
            _runtimeNotePool.Prewarm(5);
        }

        public Note.Note GetOrSpawn(RhythmAction action, float spawnTime, float appearDuration)
        {
            switch (action.noteType)
            {
                case NoteType.Runtime:
                    return SpawnRuntimeNote(spawnTime, appearDuration);

                case NoteType.Persistent:
                    return GetOrInitializePersistentNote(action.targetID, spawnTime, appearDuration);

                default:
                    return null;
            }
        }

        private Note.Note SpawnRuntimeNote(float spawnTime, float appearDuration)
        {
            Note.Note note = _runtimeNotePool.Get();
            if (note != null)
            {
                note.transform.SetAsFirstSibling();

                note.Setup(spawnTime, appearDuration);
                return note;
            }
            return null;
        }

        private Note.Note GetOrInitializePersistentNote(string id, float spawnTime, float appearDuration)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int key = id.GetHashCode();

            if (_fixedNoteMap.TryGetValue(key, out var note) && note != null)
            {
                note.gameObject.SetActive(true);
                note.ResetJudgedState();
                note.InitializePersistent(spawnTime, appearDuration);
                return note;
            }

            GameObject prefab = _presenter.GetCurrentNotePrefab();
            GameObject obj = _presenter.SpawnNote(prefab);

            if (obj != null && obj.TryGetComponent<Note.Note>(out var newNote))
            {
                newNote.InitializePersistent(spawnTime, appearDuration);
                _fixedNoteMap[key] = newNote; 
                return newNote;
            }

            return null;
        }

        /// <summary>
        /// 테마 변경 등으로 노트를 싹 정리해야 할 때 호출합니다.
        /// </summary>
        public void Reset()
        {
            // 고정 노트 비활성화
            foreach (var note in _fixedNoteMap.Values)
            {
                if (note != null) note.gameObject.SetActive(false);
            }

            _runtimeNotePool.Clear();
        }

        /// <summary>
        /// 정적(Signal 등) 노트를 ID로 찾아 반환합니다.
        /// </summary>
        public Note.Note GetStaticNote(string noteID)
        {
            if (string.IsNullOrEmpty(noteID)) return null;
            return _fixedNoteMap.TryGetValue(noteID.GetHashCode(), out var note) ? note : null;
        }
    }
}