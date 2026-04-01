#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Project.Rhythm.Data;

//OnGUI 전체의 기준점은 윈도우의 좌상단(0,0) 입니다
//BeginScrollView 내부로 들어가는 순간, 기준점은 스크롤 영역의 내부 좌상단으로 가상화됩니다
//그래서 타임라인을 그릴 때 윈도우 크기를 걱정하지 않고 "시간 * 픽셀" 공식을 그대로 쓸 수 있는 것입니다

namespace Project.Editor
{
    public class RhythmEditorWindow : EditorWindow
    {
        // 상태 변수들 (공유 데이터)
        public float pixelPerSecond = 100f;                 //초당 필셀 수
        public float bpm = 120f;                            //현재 곡의 BPM, 그리드 간격과 스냅 계산에 사용
        public Vector2 scroll;                              //타임 라인 스크롤 위치, GUI의 좌표계 offset으로 활용
        public StageData stageData;                         //편집 중인 실제 데이터
        public AudioSource previewSource;                   //곡 미리 듣기용 오디오 소스, 재생/일시정지 상태 관리에 활용
        public int selectedNoteIndex = -1;                  //선택된 노트 인덱스, -1이면 선택 없음
        public int selectedThemeEventIndex = -1;            //선택된 테마 이벤트 인덱스, -1이면 선택 없음
        public bool useMultipleThemes = true;               //테마 이벤트가 여러 개일 때, 선택된 테마 이벤트만 편집할지 여부

        //기능 분리
        private TimelineDrawer _timelineDrawer;             //타임 라인 그리기 담당 클래스, OnGUI에서 호출하여 타임 라인과 노트, 이벤트 등을 렌더링
        private InspectorDrawer _inspectorDrawer;           //인스펙터 그리기 담당 클래스, OnGUI에서 호출하여 상단 컨트롤 바와 우측 인스펙터 패널을 렌더링
        private InputHandler _inputHandler;                 //입력 처리 담당 클래스, 타임 라인에서 마우스 클릭과 키보드 입력을 감지하여 노트와 이벤트의 선택, 생성, 삭제 등을 처리

        /// <summary> 1박자(1 Beat)가 차지하는 픽셀 길이를 실시간 계산합니다. </summary>
        /// <remarks> 공식: (60초 / BPM) * 초당픽셀수. 비트 단위로 그리드를 그릴 때 사용 </remarks>
        public float PixelsPerBeat => (60f / bpm) * pixelPerSecond;

        [MenuItem("Window/Rhythm Editor")]
        public static void ShowWindow() => GetWindow<RhythmEditorWindow>("Rhythm Editor");

        /// <remarks>
        /// 유니티 에디터는 스크립트가 컴파일되거나 윈도우가 다시 포커스될 때 메모리에서 객체가 소실될 수 있다
        /// OnEnable은 이 시점에 다시 호출되므로, 여기서 Drawer들을 재생성하여 'this(현재 윈도우)'를 연결
        /// </remarks>
        private void OnEnable()
        {
            _timelineDrawer = new TimelineDrawer(this);
            _inspectorDrawer = new InspectorDrawer(this);
            _inputHandler = new InputHandler(this);
        }

        /// <summary> 
        /// IMGUI 렌더링 파이프라인. 
        /// 사용자 입력(마우스/키)이나 화면 갱신이 필요할 때마다 초당 수십 번 호출됨
        /// </summary>
        private void OnGUI()
        {
            /// <remarks> 
            /// [고정 영역] 스크롤 외부 UI. 
            /// 상단 툴바와 인스펙터 정보를 먼저 그리고 이 영역은 아래 타임라인이 스크롤되어도 위치가 고정됨
            /// </remarks>
            _inspectorDrawer.DrawControlBar();

            if (stageData == null || stageData.masterTrack == null)
            {
                EditorGUILayout.HelpBox("StageData와 곡을 등록해주세요.", MessageType.Info);
                return;
            }

            _inspectorDrawer.DrawUnifiedInspector();

            /// <remarks> 
            /// [구분선 생성] GUILayoutUtility.GetRect를 통해 자동 레이아웃 시스템 내에서 
            /// 정확히 높이 1px의 사각형 영역을 예약하고, EditorGUI.DrawRect로 색을 채웁니다. 
            /// </remarks>
            EditorGUILayout.Space(2);
            Rect lineRect = GUILayoutUtility.GetRect(position.width, 1);
            EditorGUI.DrawRect(lineRect, Color.gray);

            /// <remarks>
            /// [오토 스크롤 로직]
            /// 음악이 재생 중일 때, 플레이헤드(빨간 선)의 X좌표를 계산한 뒤
            /// 현재 윈도우 너비의 절반을 뺌으로써 '빨간 선이 항상 화면 중앙'에 오도록 scroll.x를 강제 수정합니다.
            /// </remarks>
            if (previewSource != null && previewSource.isPlaying)
            {
                float playheadX = previewSource.time * pixelPerSecond;
                scroll.x = playheadX - (position.width * 0.5f);
            }

            /// <remarks>
            /// [가변 영역] 스크롤 뷰.
            /// 이 내부에서 그려지는 TimelineDrawer의 모든 좌표는 scroll 값에 의해 상대적으로 오프셋이 적용됩니다.
            /// BeginScrollView는 사용자가 변경한 새로운 scroll 값을 반환하므로 이를 다시 저장해야 합니다.
            /// </remarks>
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _timelineDrawer.DrawTimeline();
            EditorGUILayout.EndScrollView();

            /// <remarks> 하단 가이드 툴바. </remarks>
            _inspectorDrawer.DrawGuide();

            /// <remarks> 
            /// [강제 화면 갱신] 
            /// 음악이 재생 중이거나 플레이 모드일 때 Repaint()를 호출하여 OnGUI를 강제로 다시 실행시킵니다.
            /// 이를 통해 플레이헤드가 멈추지 않고 매 프레임 부드럽게 움직이는 애니메이션 효과를 줍니다.
            /// </remarks>
            if (previewSource != null && (previewSource.isPlaying || EditorApplication.isPlaying)) Repaint();
        }
    }
}
#endif