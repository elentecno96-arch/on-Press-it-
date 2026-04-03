#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Project.Rhythm.Data;

public class ThemePopupEditor : PopupWindowContent
{
    private Project.Editor.TestEditor.Engine.EditorEngine _engine;
    private Vector2 _scrollPos;
    private int _indexToRemove = -1; // 삭제 대기 중인 인덱스

    public ThemePopupEditor(Project.Editor.TestEditor.Engine.EditorEngine engine) => _engine = engine;

    public override Vector2 GetWindowSize() => new Vector2(400, 500);

    public override void OnGUI(Rect rect)
    {
        // 배경 드로잉
        EditorGUI.DrawRect(new Rect(0, 0, rect.width, rect.height), new Color(0.2f, 0.2f, 0.2f, 1f));

        _indexToRemove = -1; // 매 프레임 초기화

        GUILayout.BeginVertical();
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("  Theme Resource Manager", EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("+ Add New Theme", GUILayout.Height(30)))
            {
                _engine.themeResources.Add(new ThemeResource());
            }

            EditorGUILayout.Space(5);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            {
                // 리스트 출력
                for (int i = 0; i < _engine.themeResources.Count; i++)
                {
                    DrawThemeItem(i);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        // [중요] 모든 GUILayout Begin/End 쌍이 완료된 후 리스트 수정 수행
        if (_indexToRemove != -1)
        {
            _engine.themeResources.RemoveAt(_indexToRemove);
            _indexToRemove = -1;
            // 리스트 구조가 바뀌었으므로 다음 프레임에 다시 그리도록 강제 유도 가능
        }
    }


    private void DrawThemeItem(int index)
    {
        var res = _engine.themeResources[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.BeginHorizontal();
            {
                res.theme = (StageThemeType)EditorGUILayout.EnumPopup(res.theme);

                // 여기서 RemoveAt을 직접 호출하지 않고 인덱스만 전달
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _indexToRemove = index;
                }
            }
            EditorGUILayout.EndHorizontal();

            // 필드 간격 조절
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            res.backgroundPrefab = (GameObject)EditorGUILayout.ObjectField("Background", res.backgroundPrefab, typeof(GameObject), false);
            res.playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player", res.playerPrefab, typeof(GameObject), false);
            res.notePrefab = (GameObject)EditorGUILayout.ObjectField("Note", res.notePrefab, typeof(GameObject), false);

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}
#endif