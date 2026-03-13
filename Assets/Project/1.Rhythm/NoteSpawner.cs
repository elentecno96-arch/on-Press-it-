using Project.Rhythm.Presentation;
using UnityEngine;

namespace Project.Rhythm
{
    /// <summary>
    /// 노트 생성 담당 클래스
    /// </summary>
    public class NoteSpawner
    {
        private readonly StagePresenter _presenter;

        public NoteSpawner(StagePresenter presenter)
        {
            _presenter = presenter;
        }

        public Project.Rhythm.Note.Note Spawn(float spawnTime, float appearDuration, bool isRandomPos = false)
        {
            GameObject obj = _presenter.SpawnNote();
            var note = obj.GetComponent<Project.Rhythm.Note.Note>();

            note.Setup(spawnTime, appearDuration, isRandomPos);

            return note;
        }
    }
}