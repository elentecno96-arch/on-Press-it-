using Project.Editor.TestEditor.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TimelineView
{
    private readonly EditorEngine _engine;

    // 비율 설정 (총합 10)
    private const float RULER_RATIO = 0.1f;  // 상단 시간축 (1)
    private const float NOTE_RATIO = 0.6f;   // 노트 영역 (6)
    private const float THEME_RATIO = 0.3f;  // 테마 영역 (3)

    public TimelineView(EditorEngine engine) => _engine = engine;

    public void Draw(Rect totalRect)
    {
        bool isDisabled = _engine.currentStageData == null;

        EditorGUI.BeginDisabledGroup(isDisabled);
        {
            float h = totalRect.height;
            float rulerHeight = h * RULER_RATIO;
            float noteHeight = h * NOTE_RATIO;
            float themeHeight = h * THEME_RATIO;

            Rect rulerArea = new Rect(totalRect.x, totalRect.y, totalRect.width, rulerHeight);
            Rect noteArea = new Rect(totalRect.x, rulerArea.yMax, totalRect.width, noteHeight);
            Rect themeArea = new Rect(totalRect.x, noteArea.yMax, totalRect.width, themeHeight);

            EditorGUI.DrawRect(rulerArea, new Color(0.2f, 0.2f, 0.2f));
            EditorGUI.DrawRect(noteArea, new Color(0.12f, 0.12f, 0.12f));
            EditorGUI.DrawRect(themeArea, new Color(0.15f, 0.15f, 0.18f));

            Rect gridArea = new Rect(totalRect.x, rulerArea.yMax, totalRect.width, noteHeight + themeHeight);
            DrawBPMGrid(gridArea);

            DrawActions(noteArea);
            DrawThemeEvents(themeArea);

            DrawRulerContent(rulerArea);

            DrawHorizontalLine(rulerArea.yMax, totalRect.width);
            DrawHorizontalLine(noteArea.yMax, totalRect.width);

            if (isDisabled)
            {
                GUI.Label(totalRect, "데이터 없음", EditorStyles.whiteLargeLabel);
            }

            DrawPlayhead(totalRect, rulerHeight, noteHeight + themeHeight);
        }
        EditorGUI.EndDisabledGroup();
    }


    private void DrawActions(Rect noteArea)
    {
        if (_engine.actions == null) return;

        for (int i = 0; i < _engine.actions.Count; i++)
        {
            var action = _engine.actions[i];
            float actionTime = _engine.BeatToTime(action.beat);
            float xPos = noteArea.x + (actionTime * _engine.zoomLevel) - _engine.scrollX;

            if (xPos < noteArea.x - 20f || xPos > noteArea.xMax + 20f) continue;

            float noteHeight = noteArea.height * 0.5f;
            float yPos = noteArea.y + (noteArea.height - noteHeight) * 0.5f;

            Rect noteRect = new Rect(xPos - 8, yPos, 16, noteHeight);

            Color noteColor;
            if (i == _engine.selectedActionIndex)
                noteColor = Color.white;
            else
                noteColor = (action.role == Project.Rhythm.Data.Struct.ActionRole.Hit) ? Color.cyan : new Color(1f, 0.8f, 0f);

            EditorGUI.DrawRect(noteRect, noteColor);
        }
    }

    private void DrawThemeEvents(Rect themeArea)
    {
        if (_engine.themeEvents == null) return;

        for (int i = 0; i < _engine.themeEvents.Count; i++)
        {
            var tEvent = _engine.themeEvents[i];
            float eventTime = _engine.BeatToTime(tEvent.beat);
            float xPos = themeArea.x + (eventTime * _engine.zoomLevel) - _engine.scrollX;

            if (xPos < themeArea.x - 20f || xPos > themeArea.xMax + 20f) continue;

            Color eventColor = (i == _engine.selectedThemeEventIndex) ? Color.white : new Color(0.8f, 0.4f, 1f); 

            Rect eventRect = new Rect(xPos - 6, themeArea.y + (themeArea.height * 0.1f), 12, themeArea.height * 0.8f);
            EditorGUI.DrawRect(eventRect, eventColor);

            GUIStyle themeLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            themeLabelStyle.normal.textColor = eventColor;
            GUI.Label(new Rect(xPos + 8, themeArea.y + 5, 100, 18), tEvent.theme.ToString(), themeLabelStyle);
        }
    }

    private void DrawRulerContent(Rect area)
    {
        Rect controlRect = new Rect(area.x, area.y, area.width, area.height * 0.7f);

        Rect scrollRect = new Rect(area.x, controlRect.yMax, area.width, area.height * 0.3f);

        DrawTopControlButtons(controlRect);

        _engine.scrollX = GUI.HorizontalScrollbar(
            scrollRect,
            _engine.scrollX,
            area.width,
            0f,
            _engine.TotalPixelWidth 
        );
    }

    private void DrawTopControlButtons(Rect area)
    {
        float sectionWidth = area.width / 3f;
        Rect leftRect = new Rect(area.x, area.y, sectionWidth, area.height);
        Rect centerRect = new Rect(area.x + sectionWidth, area.y, sectionWidth, area.height);
        Rect rightRect = new Rect(area.x + sectionWidth * 2, area.y, sectionWidth, area.height);

        GUILayout.BeginArea(leftRect);
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space(10);
            string timeStr = string.Format("{0:00}:{1:00.00}", (int)_engine.currentTime / 60, _engine.currentTime % 60);
            GUILayout.Label(timeStr, EditorStyles.boldLabel, GUILayout.Width(70));

            float currentBeat = (_engine.currentTime * _engine.bpm) / 60f;
            GUILayout.Label($"Beat: {currentBeat:F2}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(centerRect);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("|<", EditorStyles.miniButtonLeft, GUILayout.Width(30))) { _engine.currentTime = 0; }
            if (GUILayout.Button("<<", EditorStyles.miniButtonMid, GUILayout.Width(30))) { _engine.currentTime -= 1f; }

            string playLabel = _engine.isPlaying ? "||" : ">";
            if (GUILayout.Button(playLabel, EditorStyles.miniButtonMid, GUILayout.Width(40))) { _engine.isPlaying = !_engine.isPlaying; }

            if (GUILayout.Button(">>", EditorStyles.miniButtonMid, GUILayout.Width(30))) { _engine.currentTime += 1f; }

            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("RESET", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("경고", "모든 노트를 삭제하시겠습니까?", "네", "아니오"))
                {
                    _engine.actions.Clear();
                    _engine.themeEvents.Clear();
                    _engine.ClearSelection();
                }
            }
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(rightRect);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("Zoom", EditorStyles.miniLabel);
            _engine.zoomLevel = EditorGUILayout.FloatField(_engine.zoomLevel, GUILayout.Width(50));
            _engine.zoomLevel = Mathf.Clamp(_engine.zoomLevel, 10f, 1000f);

            if (GUILayout.Button("초기화", EditorStyles.miniButton, GUILayout.Width(50))) { _engine.zoomLevel = 100f; }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawBPMGrid(Rect area)
    {
        if (_engine.bpm <= 0) return;

        float beatDuration = 60f / _engine.bpm;
        float startTime = _engine.scrollX / _engine.zoomLevel;
        float endTime = (_engine.scrollX + area.width) / _engine.zoomLevel;

        float snapStep = beatDuration * 0.5f;
        int startStep = Mathf.FloorToInt(startTime / snapStep);
        int endStep = Mathf.CeilToInt(endTime / snapStep);

        for (int i = startStep; i <= endStep; i++)
        {
            float lineTime = i * snapStep;
            float xPos = area.x + (lineTime * _engine.zoomLevel) - _engine.scrollX;

            if (xPos < area.x || xPos > area.xMax) continue;

            Color lineColor;
            if (i % 8 == 0) lineColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            else if (i % 2 == 0) lineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); 
            else lineColor = new Color(0.3f, 0.3f, 0.3f, 0.2f); 

            EditorGUI.DrawRect(new Rect(xPos, area.y, 1f, area.height), lineColor);

            if (i % 8 == 0)
            {
                GUI.Label(new Rect(xPos + 5, area.y + 2, 50, 20), $"M {i / 8}", EditorStyles.boldLabel);
            }
        }
    }

    private void DrawPlayhead(Rect totalArea, float rulerH, float contentH)
    {
        float xPos = totalArea.x + (_engine.currentTime * _engine.zoomLevel) - _engine.scrollX;

        if (xPos >= totalArea.x && xPos <= totalArea.xMax)
        {
            Rect lineRect = new Rect(xPos, totalArea.y + rulerH, 1f, contentH);
            EditorGUI.DrawRect(lineRect, new Color(1f, 0f, 0f, 0.7f));
        }
    }

    private void DrawHorizontalLine(float yPos, float width)
    {
        EditorGUI.DrawRect(new Rect(0, yPos, width, 1f), new Color(0.3f, 0.3f, 0.3f));
    }
}