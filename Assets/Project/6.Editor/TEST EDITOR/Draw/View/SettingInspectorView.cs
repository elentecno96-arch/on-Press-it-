using Project.Rhythm.Data;
using Project.Rhythm.Data.Struct;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.TestEditor.Draw.View
{
    public class SettingInspectorView
    {
        private Engine.EditorEngine _engine;

        private GUIStyle _smallLabelStyle;
        private GUIStyle _smallFieldStyle;
        private GUIStyle _smallTextFieldStyle;
        private GUIStyle _smallTitleStyle;

        private bool _isInitialized = false;

        private const float NAME_PADDING = 0.4f;             // 상단 설정바 너비 비율
        private const float INPUT_PADDING = 0.3f;            // 설정바 내부 패딩

        public SettingInspectorView(Engine.EditorEngine engine) => _engine = engine;

        private void InitStyles()
        {
            if (_isInitialized) return;

            _smallLabelStyle = new GUIStyle(EditorStyles.label) { fontSize = 10 };
            _smallFieldStyle = new GUIStyle(EditorStyles.numberField) { fontSize = 10 };
            _smallTextFieldStyle = new GUIStyle(EditorStyles.textField) { fontSize = 10 };
            _smallTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 9 };

            _isInitialized = true;
        }

        public void Draw(Rect rect)
        {
            InitStyles();

            bool isDisabled = _engine.currentStageData == null;

            GUILayout.BeginArea(rect, EditorStyles.helpBox);
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        float leftPanelWidth = rect.width * NAME_PADDING;
                        EditorGUILayout.BeginVertical(GUILayout.Width(leftPanelWidth));
                        {
                            GUILayout.Label("스테이지 데이터 세팅", _smallTitleStyle);
                            EditorGUILayout.Space(2);

                            float originalLabelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = leftPanelWidth * INPUT_PADDING;

                            var prevData = _engine.currentStageData;
                            _engine.currentStageData = (StageData)EditorGUILayout.ObjectField("스테이지 SO", _engine.currentStageData, typeof(StageData), false);

                            if (prevData != _engine.currentStageData)
                            {
                                if (_engine.currentStageData != null)
                                {
                                    SyncEngineWithSO();
                                }
                                else
                                {
                                    _engine.ClearData();
                                }
                            }

                            EditorGUILayout.Space(2);

                            EditorGUI.BeginDisabledGroup(isDisabled);
                            {
                                _engine.stageName = EditorGUILayout.TextField("스테이지 이름", _engine.stageName, _smallTextFieldStyle);
                                _engine.masterTrack = (AudioClip)EditorGUILayout.ObjectField("스테이지 곡", _engine.masterTrack, typeof(AudioClip), false);

                                EditorGUILayout.Space(2);
                                _engine.bpm = EditorGUILayout.FloatField("BPM", _engine.bpm, _smallFieldStyle);
                                _engine.stageIndex = EditorGUILayout.IntField("Stage Index", _engine.stageIndex, _smallFieldStyle);
                                _engine.skipGuide = EditorGUILayout.Toggle("Skip Guide", _engine.skipGuide);
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUIUtility.labelWidth = originalLabelWidth;
                        }
                        EditorGUILayout.EndVertical();

                        DrawVerticalLine(rect.height - 10);

                        EditorGUI.BeginDisabledGroup(isDisabled);
                        {
                            float remainingWidth = rect.width - leftPanelWidth;
                            float halfRemainingWidth = (remainingWidth * 0.5f) - 10f;

                            // 중앙 패널
                            EditorGUILayout.BeginVertical(GUILayout.Width(halfRemainingWidth));
                            {
                                GUILayout.Label("저장 및 판정 설정", _smallTitleStyle);
                                EditorGUILayout.Space(2);

                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button("JSON 저장", EditorStyles.miniButtonLeft)) { _engine.SaveToJson(); }
                                    if (GUILayout.Button("JSON 로드", EditorStyles.miniButtonMid)) { _engine.LoadFromJson(); }
                                    if (GUILayout.Button("Save SO", EditorStyles.miniButtonRight)) { _engine.SaveToSO(); }
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space(2);
                                GUILayout.Label("판정 정밀 수치", EditorStyles.miniBoldLabel);
                                _engine.perfectWindow = EditorGUILayout.FloatField("Perfect", _engine.perfectWindow, _smallFieldStyle);
                                _engine.greatWindow = EditorGUILayout.FloatField("Great", _engine.greatWindow, _smallFieldStyle);
                                _engine.goodWindow = EditorGUILayout.FloatField("Good", _engine.goodWindow, _smallFieldStyle);
                                _engine.missWindow = EditorGUILayout.FloatField("Miss", _engine.missWindow, _smallFieldStyle);
                            }
                            EditorGUILayout.EndVertical();

                            DrawVerticalLine(rect.height - 10);

                            EditorGUILayout.BeginVertical(GUILayout.Width(halfRemainingWidth));
                            {
                                GUILayout.Label("테마 및 오디오", _smallTitleStyle);
                                EditorGUILayout.Space(1);

                                if (GUILayout.Button("테마 설정", GUILayout.Height(20), GUILayout.Width(halfRemainingWidth - 10)))
                                {
                                    OpenThemePopup();
                                }
                                GUILayout.Label($"활성화 된 테마: {_engine.themeResources.Count}", EditorStyles.miniLabel);

                                EditorGUILayout.Space(5);
                                DrawHorizontalLine_Local(halfRemainingWidth - 10);
                                EditorGUILayout.Space(5);

                                GUILayout.Label("오디오 출력", EditorStyles.miniBoldLabel);

                                _engine.audioSource = (AudioSource)EditorGUILayout.ObjectField(_engine.audioSource, typeof(AudioSource), true);

                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("볼륨", _smallLabelStyle, GUILayout.Width(30));
                                    float newVol = EditorGUILayout.Slider(_engine.masterVolume, 0f, 1f);
                                    if (newVol != _engine.masterVolume)
                                    {
                                        _engine.masterVolume = newVol;
                                        if (_engine.audioSource != null) _engine.audioSource.volume = newVol;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }


        private void SyncEngineWithSO()
        {
            var data = _engine.currentStageData;
            if (data == null) return;

            _engine.stageIndex = data.stageIndex;
            _engine.skipGuide = data.skipGuide;
            _engine.stageName = data.stageName;
            _engine.masterTrack = data.masterTrack;
            _engine.bpm = data.bpm;

            _engine.perfectWindow = data.perfectWindow;
            _engine.greatWindow = data.greatWindow;
            _engine.goodWindow = data.goodWindow;
            _engine.missWindow = data.missWindow;

            _engine.actions = new System.Collections.Generic.List<RhythmAction>(data.actions);
            _engine.themeEvents = new System.Collections.Generic.List<ThemeEvent>(data.themeEvents);
            _engine.themeResources = new System.Collections.Generic.List<ThemeResource>(data.themeResources);

            _engine.ClearSelection();
            if (_engine.audioSource != null)
            {
                _engine.audioSource.Stop();
                _engine.audioSource.time = 0f;
            }
        }

        private void OpenThemePopup()
        {
            var popup = new ThemePopupEditor(_engine);
            Rect windowRect = EditorWindow.focusedWindow.position;
            Vector2 popupSize = new Vector2(400, 500);
            Rect spawnRect = new Rect((windowRect.width - popupSize.x) * 0.5f, (windowRect.height - popupSize.y) * 0.5f, 10, 10);
            PopupWindow.Show(spawnRect, popup);
        }

        private void DrawVerticalLine(float height)
        {
            Color lineColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            Rect lineRect = EditorGUILayout.GetControlRect(false, height, GUILayout.Width(1));
            EditorGUI.DrawRect(lineRect, lineColor);
        }

        private void DrawHorizontalLine_Local(float width)
        {
            Color lineColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            Rect lineRect = GUILayoutUtility.GetRect(width, 1);
            EditorGUI.DrawRect(lineRect, lineColor);
        }
    }
}