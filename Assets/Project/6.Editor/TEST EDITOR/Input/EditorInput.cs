using Project.Editor.TestEditor.Engine;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.TestEditor.Input
{
    public class EditorInput
    {
        private readonly EditorEngine _engine;

        private bool _isDraggingRuler = false;
        private Vector2 _lastMousePosition;

        public EditorInput(EditorEngine engine) => _engine = engine;

        public void ProcessEvents(Event e, Rect timelineRect)
        {
            if (_engine.currentStageData == null) return;

            HandleGlobalShortcuts(e);

            if (e.rawType == EventType.MouseUp) _isDraggingRuler = false;

            if (timelineRect.Contains(e.mousePosition))
            {
                float h = timelineRect.height;
                float rulerHeight = h * 0.1f;
                Rect rulerRect = new Rect(timelineRect.x, timelineRect.y, timelineRect.width, rulerHeight);

                if (rulerRect.Contains(e.mousePosition))
                {
                    if (e.type == EventType.ScrollWheel) { e.Use(); return; }

                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        _isDraggingRuler = true;
                        _lastMousePosition = e.mousePosition;
                    }

                    if (_isDraggingRuler && e.type == EventType.MouseDrag)
                    {
                        float deltaX = e.mousePosition.x - _lastMousePosition.x;
                        _engine.scrollX -= deltaX;
                        _engine.scrollX = Mathf.Max(0, _engine.scrollX);
                        _lastMousePosition = e.mousePosition;
                        e.Use();
                        EditorWindow.focusedWindow?.Repaint();
                    }
                    return; 
                }
                HandleTimelineEvents(e, timelineRect);
            }
        }

        private void HandleGlobalShortcuts(Event e)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                _engine.isPlaying = !_engine.isPlaying;
                e.Use();
                EditorWindow.focusedWindow?.Repaint();
            }
        }
        private void HandleTimelineEvents(Event e, Rect rect)
        {
            float h = rect.height;
            float rulerLimit = rect.y + (h * 0.1f);
            float noteLimit = rulerLimit + (h * 0.6f); 

            float rawTime = _engine.ScreenToTime(e.mousePosition.x, rect);
            float snappedTime = _engine.GetSnappedTime(rawTime);
            float beat = _engine.TimeToBeat(snappedTime);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (e.mousePosition.y >= rulerLimit && e.mousePosition.y < noteLimit)
                {
                    int foundIndex = _engine.actions.FindIndex(a => Mathf.Abs(a.beat - beat) < 0.1f);
                    if (foundIndex != -1)
                    {
                        _engine.selectedActionIndex = foundIndex;
                        _engine.selectedThemeEventIndex = -1;
                    }
                    else
                    {
                        _engine.AddNote(snappedTime, 0);
                        _engine.selectedActionIndex = _engine.actions.Count - 1;
                    }
                }
                else if (e.mousePosition.y >= noteLimit && e.mousePosition.y <= rect.yMax)
                {
                    int foundIndex = _engine.themeEvents.FindIndex(t => Mathf.Abs(t.beat - beat) < 0.1f);
                    if (foundIndex != -1)
                    {
                        _engine.selectedThemeEventIndex = foundIndex;
                        _engine.selectedActionIndex = -1;
                    }
                    else
                    {
                        _engine.AddThemeEvent(snappedTime);
                        _engine.selectedThemeEventIndex = _engine.themeEvents.Count - 1;
                    }
                }
                e.Use();
            }

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                if (e.mousePosition.y >= rulerLimit && e.mousePosition.y < noteLimit)
                {
                    _engine.RemoveNote(rawTime);
                }
                else if (e.mousePosition.y >= noteLimit && e.mousePosition.y <= rect.yMax)
                {
                    _engine.RemoveThemeEvent(rawTime);
                }
                e.Use();
            }
        }
    }
}