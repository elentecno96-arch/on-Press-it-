using Project.Editor.TestEditor.Engine;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using UnityEditor;
using UnityEngine;

public class NoteInspectorView
{
    private readonly EditorEngine _engine;

    public NoteInspectorView(EditorEngine engine) => _engine = engine;

    public void Draw(Rect rect)
    {
        GUILayout.BeginArea(rect, EditorStyles.helpBox);
        {
            GUILayout.Label("NoteInspector View", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (_engine.selectedActionIndex >= 0 && _engine.selectedActionIndex < _engine.actions.Count)
            {
                DrawActionInspector();
            }
            else if (_engine.selectedThemeEventIndex >= 0 && _engine.selectedThemeEventIndex < _engine.themeEvents.Count)
            {
                DrawThemeEventInspector();
            }
            else
            {
                EditorGUILayout.HelpBox("타임라인에서 편집할 노트나 테마 이벤트를 선택하세요.", MessageType.Info);
            }
        }
        GUILayout.EndArea();
    }


    private void DrawActionInspector()
    {
        var action = _engine.actions[_engine.selectedActionIndex];

        EditorGUILayout.LabelField("<color=cyan>노트 설정</color>", new GUIStyle(EditorStyles.label) { richText = true });

        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80f;

        EditorGUILayout.BeginVertical(EditorStyles.textArea);
        {
            EditorGUI.BeginChangeCheck();

            action.beat = EditorGUILayout.FloatField("Beat", action.beat);
            action.type = (PatternType)EditorGUILayout.EnumPopup("입력 타입", action.type);
            action.role = (ActionRole)EditorGUILayout.EnumPopup("역할", action.role);
            action.noteType = (NoteType)EditorGUILayout.EnumPopup("노트 형태", action.noteType);

            if (action.type == PatternType.Hold)
            {
                action.duration = EditorGUILayout.FloatField("Duration", action.duration);
            }

            action.targetID = EditorGUILayout.TextField("Target ID", action.targetID);

            if (EditorGUI.EndChangeCheck())
            {
                _engine.actions[_engine.selectedActionIndex] = action;
                EditorUtility.SetDirty(_engine.currentStageData);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUIUtility.labelWidth = originalLabelWidth;

        if (GUILayout.Button("선택 해제", GUILayout.Height(30))) _engine.ClearSelection();
    }

    private void DrawThemeEventInspector()
    {
        var tEvent = _engine.themeEvents[_engine.selectedThemeEventIndex];

        EditorGUILayout.LabelField("<color=orange>테마 이벤트 설정</color>", new GUIStyle(EditorStyles.label) { richText = true });

        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80f; 

        EditorGUILayout.BeginVertical(EditorStyles.textArea);
        {
            EditorGUI.BeginChangeCheck();

            tEvent.beat = EditorGUILayout.FloatField("Beat", tEvent.beat);
            tEvent.theme = (StageThemeType)EditorGUILayout.EnumPopup("변경 테마", tEvent.theme);

            if (EditorGUI.EndChangeCheck())
            {
                _engine.themeEvents[_engine.selectedThemeEventIndex] = tEvent;
                EditorUtility.SetDirty(_engine.currentStageData);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUIUtility.labelWidth = originalLabelWidth;

        if (GUILayout.Button("선택 해제", GUILayout.Height(30))) _engine.ClearSelection();
    }
}