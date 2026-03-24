using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using Project.Rhythm.Presentation;
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

        public NoteSpawnSystem(StagePresenter presenter)
        {
            _presenter = presenter;
        }

        public Note.Note GetOrSpawn(RhythmAction action, float spawnTime, float appearDuration)
        {
            if (action.noteType != NoteType.Runtime)
            {
                Debug.LogError("Runtime 노트만 Spawn 가능");
                return null;
            }

            GameObject obj = _presenter.SpawnNote();
            if (obj == null) return null;

            if (obj.TryGetComponent<Note.Note>(out var note))
            {
                note.Setup(spawnTime, appearDuration);
                return note;
            }

            return null;
        }

        public void Reset()
        {

        }
        /// <summary>
        /// 정적 노트를 위한 함수
        /// </summary>
        /// <param name="noteID"></param>
        /// <returns></returns>
        public Note.Note GetStaticNote(string noteID)
        {
            return _presenter.GetFixedNote(noteID);
        }
    }
}