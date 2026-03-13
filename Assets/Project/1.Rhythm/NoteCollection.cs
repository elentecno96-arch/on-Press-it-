using Project.Rhythm.Note;
using System.Collections.Generic;

namespace Project.Rhythm
{
    /// <summary>
    /// id와 키로 노트를 등록 및 찾기
    /// </summary>
    public class NoteCollection
    {
        private readonly Dictionary<int, Note.Note> _persistentNotes = new();

        public void Register(string id, Note.Note note)
        {
            int hash = id.GetHashCode();
            if (!_persistentNotes.ContainsKey(hash))
                _persistentNotes.Add(hash, note);
        }

        public Note.Note GetNote(string id) => _persistentNotes.TryGetValue(id.GetHashCode(), out var note) ? note : null;
    }
}
