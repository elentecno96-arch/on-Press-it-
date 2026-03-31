using Project.Editor.TestEditor.Draw;
using Project.Editor.TestEditor.Engine;
using Project.Editor.TestEditor.Input;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.TestEditor
{
    /// <summary>
    /// 에디터 본채, 관리하는 클래스
    /// </summary>
    public class NewRhythmEditor : EditorWindow
    {
        private EditorEngine _engine;
        private EditorDraw _draw;
        private EditorInput _input;

        //해상도 수치
        private const float FIXED_WIDTH = 920f;     // 원하는 고정 너비
        private const float FIXED_HEIGHT = 600f;    // 원하는 고정 높이

        [MenuItem("Window/New Rhythm Editor")]
        public static void ShowWindow()
        {
            NewRhythmEditor window = GetWindow<NewRhythmEditor>("New Editor");

            Vector2 fixedSize = new Vector2(FIXED_WIDTH, FIXED_HEIGHT);
            window.minSize = fixedSize;
            window.maxSize = fixedSize;
        }

        private void OnEnable()
        {
            _engine = new EditorEngine();
            _draw = new EditorDraw(_engine);
            _input = new EditorInput(_engine);
        }

        private void OnGUI()
        {
            Rect canvasRect = new Rect(0, 0, FIXED_WIDTH, FIXED_HEIGHT);
            EditorGUI.DrawRect(canvasRect, new Color(0.15f, 0.15f, 0.15f, 1f));

            _engine.OnUpdate();

            Rect timelineRect = new Rect(0, 140f, FIXED_WIDTH - 210f, FIXED_HEIGHT - 140f);

            Event e = Event.current;

            bool shouldRepaint = _engine.isPlaying ||
                                 e.type == EventType.MouseDown ||
                                 e.type == EventType.MouseDrag ||
                                 e.type == EventType.MouseUp ||
                                 e.type == EventType.ScrollWheel;

            _input.ProcessEvents(e, timelineRect);

            _draw.OnDraw(canvasRect);

            if (shouldRepaint)
            {
                Repaint();
            }
        }
    }
}