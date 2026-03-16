using Project.Rhythm.Data.Struct;
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
            if (!string.IsNullOrEmpty(action.targetID))
            {
                return _presenter.GetFixedNote(action.targetID);
            }

            // 2. 일반적인 노트 생성 로직
            GameObject obj = _presenter.SpawnNote();
            if (obj == null) return null;

            if (obj.TryGetComponent<Note.Note>(out var note))
            {

                note.Setup(spawnTime, appearDuration);
                return note;
            }
            return null;
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