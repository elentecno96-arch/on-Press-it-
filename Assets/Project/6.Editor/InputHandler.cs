#if UNITY_EDITOR
using Project.Rhythm.Data;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{

    public class InputHandler
    {
        private readonly RhythmEditorWindow _w;

        public InputHandler(RhythmEditorWindow window) => _w = window;

        public void HandleInput(Rect rect, float noteTrackHeight)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                if (_w.previewSource.isPlaying) _w.previewSource.Pause(); else _w.previewSource.Play();
                e.Use();
            }

            if (!rect.Contains(e.mousePosition)) return;

            float relativeX = e.mousePosition.x - rect.x;
            float relativeY = e.mousePosition.y - rect.y;
            float snappedBeat = Mathf.Round((relativeX / _w.PixelsPerBeat) * 2f) / 2f;
            bool isThemeLane = relativeY > noteTrackHeight;

            if (e.type == EventType.MouseDown)
            {
                Undo.RecordObject(_w.stageData, "Modify Rhythm Data");
                if (e.button == 0) // 선택
                {
                    if (isThemeLane)
                    {
                        _w.selectedThemeEventIndex = _w.stageData.themeEvents.FindIndex(t => Mathf.Approximately(t.beat, snappedBeat));
                        _w.selectedNoteIndex = -1;
                    }
                    else
                    {
                        _w.selectedNoteIndex = _w.stageData.actions.FindIndex(a => Mathf.Abs(a.beat - snappedBeat) < 0.05f);
                        _w.selectedThemeEventIndex = -1;
                    }
                }
                else if (e.button == 1) // 생성/삭제
                {
                    if (isThemeLane) HandleThemeEventToggle(snappedBeat);
                    else HandleNoteToggle(snappedBeat);
                }
                EditorUtility.SetDirty(_w.stageData);
                e.Use();
            }
        }

        private void HandleNoteToggle(float beat)
        {
            int idx = _w.stageData.actions.FindIndex(a => Mathf.Approximately(a.beat, beat));
            if (idx != -1) _w.stageData.actions.RemoveAt(idx);
            else _w.stageData.actions.Add(new RhythmAction { beat = beat, noteType = NoteType.Runtime });
            _w.stageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));
        }

        private void HandleThemeEventToggle(float beat)
        {
            int idx = _w.stageData.themeEvents.FindIndex(t => Mathf.Approximately(t.beat, beat));
            if (idx != -1) _w.stageData.themeEvents.RemoveAt(idx);
            else _w.stageData.themeEvents.Add(new ThemeEvent { beat = beat, theme = StageThemeType.Stage1 });
            _w.stageData.themeEvents.Sort((a, b) => a.beat.CompareTo(b.beat));
        }
    }
}
#endif