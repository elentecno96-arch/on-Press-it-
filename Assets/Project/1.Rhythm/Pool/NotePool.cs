using System;
using UnityEngine.Pool;

namespace Project.Rhythm.Pool
{
    public class NotePool
    {
        private readonly IObjectPool<Note.Note> _pool;
        private readonly Func<Note.Note> _createFunc;

        public NotePool(Func<Note.Note> createFunc, int defaultCapacity = 20, int maxSize = 100)
        {
            _createFunc = createFunc;

            _pool = new ObjectPool<Note.Note>(
                createFunc: OnCreateInternal,
                actionOnGet: OnGetInternal,
                actionOnRelease: OnReleaseInternal,
                actionOnDestroy: OnDestroyInternal,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private Note.Note OnCreateInternal()
        {
            var note = _createFunc.Invoke();
            // 노트가 스스로 반납될 때 호출할 액션 연결
            note.SetPoolAction(Release);
            return note;
        }

        public void Prewarm(int count)
        {
            Note.Note[] tempArray = new Note.Note[count];

            for (int i = 0; i < count; i++)
            {
                tempArray[i] = Get();
            }

            for (int i = 0; i < count; i++)
            {
                Release(tempArray[i]); 
            }
        }

        private void OnGetInternal(Note.Note note) => note.gameObject.SetActive(true);

        private void OnReleaseInternal(Note.Note note)
        {
            note.ResetJudgedState();
            note.gameObject.SetActive(false);
        }

        private void OnDestroyInternal(Note.Note note) => UnityEngine.Object.Destroy(note.gameObject);

        public Note.Note Get() => _pool.Get();

        public void Release(Note.Note note) => _pool.Release(note);

        public void Clear() => _pool.Clear();
    }
}
