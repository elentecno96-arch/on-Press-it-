#if UNITY_EDITOR
using Project.Rhythm.Data.Struct;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{

    public class TimelineDrawer
    {
        private readonly RhythmEditorWindow _w;

        public TimelineDrawer(RhythmEditorWindow window) => _w = window;

        public void DrawTimeline()
        {
            float totalWidth = _w.stageData.masterTrack.length * _w.pixelPerSecond;
            float dynamicHeight = Mathf.Max(220f, _w.position.height - 300f);

            Rect rect = GUILayoutUtility.GetRect(totalWidth, dynamicHeight);
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

            float noteTrackHeight = rect.height * 0.7f;
            float themeTrackY = rect.y + noteTrackHeight;

            // 테마 레인 구분선
            EditorGUI.DrawRect(new Rect(rect.x, themeTrackY, rect.width, 2), new Color(0.4f, 0.4f, 0.4f, 1f));

            DrawPlaybackRange(rect);
            DrawGrid(rect);
            DrawNotes(rect, noteTrackHeight);
            DrawThemeEvents(rect, noteTrackHeight);

            // 입력 핸들링 호출
            new InputHandler(_w).HandleInput(rect, noteTrackHeight);

            DrawPlayhead(rect);
        }

        private void DrawPlaybackRange(Rect rect)
        {
            float startX = rect.x + (_w.stageData.playStartTime * _w.pixelPerSecond);
            float endX = rect.x + (_w.stageData.endPosition * _w.pixelPerSecond);
            EditorGUI.DrawRect(new Rect(startX, rect.y, endX - startX, rect.height), new Color(1f, 1f, 1f, 0.03f));
            Handles.color = Color.green; Handles.DrawLine(new Vector3(startX, rect.y), new Vector3(startX, rect.yMax), 2f);
            Handles.color = new Color(0.6f, 0.3f, 0.9f); Handles.DrawLine(new Vector3(endX, rect.y), new Vector3(endX, rect.yMax), 2f);
        }

        private void DrawGrid(Rect rect)
        {
            float beatInterval = 60f / _w.bpm;
            int totalBeats = Mathf.FloorToInt(_w.stageData.masterTrack.length / beatInterval);
            for (int i = 0; i <= totalBeats; i++)
            {
                float xPos = rect.x + (i * beatInterval * _w.pixelPerSecond);
                bool isBar = i % 4 == 0;
                Handles.color = isBar ? new Color(0.5f, 0.5f, 0.5f, 0.8f) : new Color(0.3f, 0.3f, 0.3f, 0.3f);
                Handles.DrawLine(new Vector3(xPos, rect.y), new Vector3(xPos, rect.yMax));
                if (isBar) GUI.Label(new Rect(xPos + 5, rect.y + 2, 50, 20), $"M {i / 4}", EditorStyles.boldLabel);
            }
        }

        private void DrawNotes(Rect rect, float trackHeight)
        {
            for (int i = 0; i < _w.stageData.actions.Count; i++)
            {
                var action = _w.stageData.actions[i];
                float xPos = rect.x + (action.beat * _w.PixelsPerBeat);
                Color col = (action.role == ActionRole.Signal) ? Color.cyan : new Color(1f, 0.8f, 0f);
                if (i == _w.selectedNoteIndex) col = Color.white;
                float nHeight = trackHeight * 0.7f;
                EditorGUI.DrawRect(new Rect(xPos - 8, rect.y + (trackHeight - nHeight) * 0.5f, 16, nHeight), col);
            }
        }

        private void DrawThemeEvents(Rect rect, float noteHeight)
        {
            float themeZoneHeight = rect.height - noteHeight;
            for (int i = 0; i < _w.stageData.themeEvents.Count; i++)
            {
                var tEvent = _w.stageData.themeEvents[i];
                float xPos = rect.x + (tEvent.beat * _w.PixelsPerBeat);
                Rect eventRect = new Rect(xPos - 6, rect.y + noteHeight + (themeZoneHeight * 0.2f), 12, themeZoneHeight * 0.6f);
                EditorGUI.DrawRect(eventRect, i == _w.selectedThemeEventIndex ? Color.white : new Color(0f, 0.8f, 1f));
            }
        }

        private void DrawPlayhead(Rect rect)
        {
            if (_w.previewSource == null) return;
            float xPos = rect.x + (_w.previewSource.time * _w.pixelPerSecond);
            Handles.color = Color.red; Handles.DrawLine(new Vector3(xPos, rect.y), new Vector3(xPos, rect.yMax), 2f);
        }
    }
}
#endif  