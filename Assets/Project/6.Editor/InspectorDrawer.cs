using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using Project.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Project.Editor
{

    public class InspectorDrawer
    {
        private readonly RhythmEditorWindow _w;

        public InspectorDrawer(RhythmEditorWindow window) => _w = window;

        public void DrawControlBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            _w.stageData = (StageData)EditorGUILayout.ObjectField(_w.stageData, typeof(StageData), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (_w.stageData != null)
                {
                    _w.bpm = _w.stageData.bpm;
                    if (_w.previewSource != null) _w.previewSource.clip = _w.stageData.masterTrack;
                    _w.useMultipleThemes = false;
                }
            }

            EditorGUILayout.Space(5);

            // --- [신규] JSON 저장/불러오기 버튼 ---
            if (_w.stageData != null)
            {
                if (GUILayout.Button("JSON 저장", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    ExportToJson();
                }
                if (GUILayout.Button("JSON 로드", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    ImportFromJson();
                }
            }
            // ------------------------------------

            EditorGUILayout.Space(5);

            _w.previewSource = (AudioSource)EditorGUILayout.ObjectField(_w.previewSource, typeof(AudioSource), true);

            if (GUILayout.Button(_w.previewSource != null && _w.previewSource.isPlaying ? "멈춤" : "시작", EditorStyles.toolbarButton, GUILayout.Width(50))) TogglePlay();
            if (GUILayout.Button("정지", EditorStyles.toolbarButton, GUILayout.Width(40))) StopMusic();

            if (_w.stageData != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Start", GUILayout.Width(35));
                float newStart = EditorGUILayout.FloatField(_w.stageData.playStartTime, GUILayout.Width(40));

                EditorGUILayout.LabelField("End", GUILayout.Width(30));
                float newEnd = EditorGUILayout.FloatField(_w.stageData.endPosition, GUILayout.Width(40));

                EditorGUILayout.LabelField("BPM", GUILayout.Width(35));
                float newBpm = EditorGUILayout.FloatField(_w.stageData.bpm, GUILayout.Width(40));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_w.stageData, "Edit Stage Global Data");
                    _w.stageData.playStartTime = newStart;
                    _w.stageData.endPosition = newEnd;
                    _w.stageData.bpm = newBpm;
                    _w.bpm = newBpm;
                    EditorUtility.SetDirty(_w.stageData);
                }
            }

            EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
            _w.pixelPerSecond = EditorGUILayout.FloatField(_w.pixelPerSecond, GUILayout.Width(40));

            EditorGUILayout.EndHorizontal();
        }

        public void DrawUnifiedInspector()
        {
            if (_w.stageData == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(150));


            _w.useMultipleThemes = EditorGUILayout.ToggleLeft("다중 테마 리소스 활성화 (Stage 2 이상)", _w.useMultipleThemes, GUILayout.Width(250));

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Theme", EditorStyles.miniBoldLabel, GUILayout.Width(65));
            EditorGUILayout.LabelField("Background Prefab", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Player Prefab", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Note Prefab", EditorStyles.miniBoldLabel);
            EditorGUILayout.EndHorizontal();

            var themeTypes = System.Enum.GetValues(typeof(StageThemeType));
            for (int i = 0; i < themeTypes.Length; i++)
            {
                StageThemeType type = (StageThemeType)themeTypes.GetValue(i);

                if (_w.stageData.themeResources == null)
                    _w.stageData.themeResources = new System.Collections.Generic.List<ThemeResource>();

                if (_w.stageData.themeResources.Count <= i)
                    _w.stageData.themeResources.Add(new ThemeResource { theme = type });

                bool isEnabled = (i == 0) || _w.useMultipleThemes;

                EditorGUI.BeginDisabledGroup(!isEnabled);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(type.ToString(), GUILayout.Width(65));

                var res = _w.stageData.themeResources[i];

                EditorGUI.BeginChangeCheck();
                // 각 필드에 어떤 프리팹을 넣어야 하는지 순서대로 배치
                res.backgroundPrefab = (GameObject)EditorGUILayout.ObjectField(res.backgroundPrefab, typeof(GameObject), false);
                res.playerPrefab = (GameObject)EditorGUILayout.ObjectField(res.playerPrefab, typeof(GameObject), false);
                res.notePrefab = (GameObject)EditorGUILayout.ObjectField(res.notePrefab, typeof(GameObject), false);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_w.stageData, "Edit Theme Resources");
                    EditorUtility.SetDirty(_w.stageData);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(5);

            if (_w.selectedNoteIndex != -1 || _w.selectedThemeEventIndex != -1) DrawSelectionDetails();
            else EditorGUILayout.LabelField("노트나 테마 이벤트를 선택하세요.", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectionDetails()
        {
            if (_w.selectedThemeEventIndex != -1 && _w.selectedThemeEventIndex < _w.stageData.themeEvents.Count)
            {
                var tEvent = _w.stageData.themeEvents[_w.selectedThemeEventIndex];
                EditorGUILayout.LabelField($"테마 이벤트 설정 (Beat: {tEvent.beat})", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                tEvent.theme = (StageThemeType)EditorGUILayout.EnumPopup("전환할 테마", tEvent.theme);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_w.stageData, "Edit Theme Event");
                    _w.stageData.themeEvents[_w.selectedThemeEventIndex] = tEvent;
                    EditorUtility.SetDirty(_w.stageData);
                }
            }
            else if (_w.selectedNoteIndex != -1 && _w.selectedNoteIndex < _w.stageData.actions.Count)
            {
                var action = _w.stageData.actions[_w.selectedNoteIndex];
                EditorGUILayout.LabelField($"노트 설정 (Beat: {action.beat})", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                action.beat = EditorGUILayout.FloatField("Beat Position", action.beat);
                action.role = (ActionRole)EditorGUILayout.EnumPopup("Action Role", action.role);
                action.noteType = (NoteType)EditorGUILayout.EnumPopup("Note Type", action.noteType);
                action.type = (PatternType)EditorGUILayout.EnumPopup("Pattern Type", action.type);
                action.duration = EditorGUILayout.FloatField("Hold Duration", action.duration);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_w.stageData, "Edit Note");
                    _w.stageData.actions[_w.selectedNoteIndex] = action;
                    _w.stageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));
                    EditorUtility.SetDirty(_w.stageData);
                }
            }
        }

        public void DrawGuide()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("좌: 선택/이동 | 우: 생성/삭제 | Space: 재생", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Track: 상단(Rhythm Action) / 하단(Theme Event)", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void TogglePlay()
        {
            if (_w.previewSource == null) return;
            if (_w.previewSource.isPlaying) _w.previewSource.Pause();
            else _w.previewSource.Play();
        }

        private void StopMusic()
        {
            if (_w.previewSource == null) return;
            _w.previewSource.Stop();
            _w.previewSource.time = 0;
            _w.scroll.x = 0;
        }

        private void ExportToJson()
        {
            string path = EditorUtility.SaveFilePanel("Save Stage Data to JSON", "Assets", _w.stageData.name + "_Notes", "json");
            if (string.IsNullOrEmpty(path)) return;

            StageDataJsonWrapper wrapper = new StageDataJsonWrapper
            {
                actions = _w.stageData.actions,
                themeEvents = _w.stageData.themeEvents
            };

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"[RhythmEditor] JSON 저장 완료: {path}");
        }

        private void ImportFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Load Stage Data from JSON", "Assets", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = File.ReadAllText(path);
            StageDataJsonWrapper wrapper = JsonUtility.FromJson<StageDataJsonWrapper>(json);

            if (wrapper != null)
            {
                Undo.RecordObject(_w.stageData, "Import JSON Data");
                _w.stageData.actions = wrapper.actions ?? new List<RhythmAction>();
                _w.stageData.themeEvents = wrapper.themeEvents ?? new List<ThemeEvent>();

                // 데이터 무결성을 위해 정렬
                _w.stageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));
                _w.stageData.themeEvents.Sort((a, b) => a.beat.CompareTo(b.beat));

                EditorUtility.SetDirty(_w.stageData);
                Debug.Log($"[RhythmEditor] JSON 로드 완료: {path}");
            }
        }
    }
}