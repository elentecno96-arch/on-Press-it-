#if UNITY_EDITOR
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

        // [MenuItem("/")]
        // 에디터 상단에 에디터를 실행 시킬 수 있는 메뉴를 추가하기 위함
        [MenuItem("Window/New Rhythm Editor")]
        public static void ShowWindow()
        {
            // GetWindow<T>
            // T 타입의 에디터 창을 열거나, 이미 열려 있다면 해당 창을 포커싱하는 메서드
            // 싱글톤 패턴 처럼 객체를 하나만 만들어서 사용하기 위함
            NewRhythmEditor window = GetWindow<NewRhythmEditor>("New Editor");

            Vector2 fixedSize = new (FIXED_WIDTH, FIXED_HEIGHT);
            window.minSize = fixedSize;
            window.maxSize = fixedSize;
        }

        private void OnEnable()
        {
            _engine = new EditorEngine();
            _draw = new EditorDraw(_engine);
            _input = new EditorInput(_engine);
        }

        // 유니티는 에디터 화면을 그릴 때 IMUI(Immediate Mode GUI) 방식을 사용하는데
        // OnGUI는 마우스 클릭 하나, 마우스 움직임 하나하나마다 호출되어 화면을 다시 그림
        private void OnGUI()
        {
            // 기본 배경 그리기
            Rect canvasRect = new (0, 0, FIXED_WIDTH, FIXED_HEIGHT);
            EditorGUI.DrawRect(canvasRect, new Color(0.15f, 0.15f, 0.15f, 1f));

            // 기능 업데이트
            _engine.OnUpdate();

            // 타임라인 영역
            Rect timelineRect = new (0, 140f, FIXED_WIDTH - 210f, FIXED_HEIGHT - 140f);

            // IMGUI 이벤트 처리
            // 유니티는 OS로 부터 오는 신호를 받아 Event 객체에 담아 전달
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
            // EditorApplication.update 루프에 Repaint를 등록해 프레임을 고정 시키면
            // 더 부드럽게 가능하나 성능을 많이 잡아 먹음
        }
    }
}
#endif