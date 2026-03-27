using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RhythmEditorWindow : EditorWindow
{
    private float pixelPerSecond = 100f;
    private float bpm = 120f;
    private Vector2 scroll;

    private StageData stageData;
    private AudioClip musicClip;
    private AudioSource previewSource;

    private int selectedNoteIndex = -1;
    private int selectedThemeEventIndex = -1;
    private bool useMultipleThemes = true;

    private float PixelsPerBeat => (60f / bpm) * pixelPerSecond;

    [MenuItem("Window/Rhythm Editor")]
    public static void ShowWindow() => GetWindow<RhythmEditorWindow>("Rhythm Editor");

    private void OnGUI()
    {
        DrawControlBar();

        if (musicClip == null || stageData == null)
        {
            EditorGUILayout.HelpBox("StageData와 곡을 등록해주세요.", MessageType.Info);
            return;
        }

        DrawUnifiedInspector();

        EditorGUILayout.Space(2);
        Rect lineRect = GUILayoutUtility.GetRect(position.width, 1);
        EditorGUI.DrawRect(lineRect, Color.gray);

        if (previewSource != null && previewSource.isPlaying)
        {
            float playheadX = previewSource.time * pixelPerSecond;
            scroll.x = playheadX - (position.width * 0.5f);
        }

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        try
        {
            float totalWidth = musicClip.length * pixelPerSecond;

            float dynamicTimelineHeight = position.height - 280f;
            dynamicTimelineHeight = Mathf.Max(220f, dynamicTimelineHeight);

            Rect timelineRect = GUILayoutUtility.GetRect(totalWidth, dynamicTimelineHeight);

            EditorGUI.DrawRect(timelineRect, new Color(0.12f, 0.12f, 0.12f, 1f));

            float noteTrackHeight = timelineRect.height * 0.7f;
            float themeTrackY = timelineRect.y + noteTrackHeight;

            EditorGUI.DrawRect(new Rect(timelineRect.x, themeTrackY, timelineRect.width, 2), new Color(0.4f, 0.4f, 0.4f, 1f));
            GUI.Label(new Rect(timelineRect.x + 5, themeTrackY + 2, 120, 20), "Theme Switching Lane", EditorStyles.miniLabel);

            DrawPlaybackRange(timelineRect);
            DrawGrid(timelineRect);
            DrawNotes(timelineRect, noteTrackHeight);
            DrawThemeEvents(timelineRect, noteTrackHeight);

            HandleInput(timelineRect, noteTrackHeight);
            DrawPlayhead(timelineRect);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Editor Layout Error Caught: {ex.Message}");
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }

        DrawGuide();

        if (previewSource != null && (previewSource.isPlaying || EditorApplication.isPlaying)) Repaint();
    }

    private void DrawUnifiedInspector()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(150));

        useMultipleThemes = EditorGUILayout.ToggleLeft("다중 테마 리소스 활성화 (Stage 2 이상)", useMultipleThemes, GUILayout.Width(250));

        EditorGUILayout.Space(2);

        var themeTypes = System.Enum.GetValues(typeof(StageThemeType));
        for (int i = 0; i < themeTypes.Length; i++)
        {
            StageThemeType type = (StageThemeType)themeTypes.GetValue(i);

            if (stageData.themeResources == null) stageData.themeResources = new List<ThemeResource>();
            if (stageData.themeResources.Count <= i) stageData.themeResources.Add(new ThemeResource { theme = type });

            bool isEnabled = (i == 0) || useMultipleThemes;
            EditorGUI.BeginDisabledGroup(!isEnabled);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(type.ToString(), GUILayout.Width(65));

            var res = stageData.themeResources[i];
            res.backgroundPrefab = (GameObject)EditorGUILayout.ObjectField(res.backgroundPrefab, typeof(GameObject), false);
            res.playerPrefab = (GameObject)EditorGUILayout.ObjectField(res.playerPrefab, typeof(GameObject), false);
            res.notePrefab = (GameObject)EditorGUILayout.ObjectField(res.notePrefab, typeof(GameObject), false);
            stageData.themeResources[i] = res;

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space(5);

        if (selectedNoteIndex != -1 || selectedThemeEventIndex != -1) DrawSelectionDetails();
        else EditorGUILayout.LabelField("노트나 테마 이벤트를 선택하세요.", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.EndVertical();
    }

    private void DrawThemeEvents(Rect rect, float noteHeight)
    {
        if (stageData?.themeEvents == null) return;
        for (int i = 0; i < stageData.themeEvents.Count; i++)
        {
            var tEvent = stageData.themeEvents[i];
            float xPos = rect.x + (tEvent.beat * PixelsPerBeat);
            float themeZoneHeight = rect.height - noteHeight;
            Rect eventRect = new Rect(xPos - 6, rect.y + noteHeight + (themeZoneHeight * 0.2f), 12, themeZoneHeight * 0.6f);
            Color col = i == selectedThemeEventIndex ? Color.white : new Color(0f, 0.8f, 1f);
            EditorGUI.DrawRect(eventRect, col);
            GUI.Label(new Rect(xPos + 8, eventRect.y, 60, 20), tEvent.theme.ToString(), EditorStyles.miniLabel);
        }
    }

    private void DrawSelectionDetails()
    {
        if (selectedThemeEventIndex != -1 && selectedThemeEventIndex < stageData.themeEvents.Count)
        {
            var tEvent = stageData.themeEvents[selectedThemeEventIndex];

            EditorGUILayout.LabelField($"테마 이벤트 설정 (Beat: {tEvent.beat})", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newBpm = EditorGUILayout.FloatField(bpm, GUILayout.Width(40));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(stageData, "Change BPM");
                stageData.bpm = newBpm;
                bpm = newBpm;
                EditorUtility.SetDirty(stageData);
            }

            tEvent.theme = (StageThemeType)EditorGUILayout.EnumPopup("전환할 테마", tEvent.theme);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(stageData, "Edit Theme Event");

                stageData.themeEvents[selectedThemeEventIndex] = tEvent;

                EditorUtility.SetDirty(stageData);
            }
        }
        else if (selectedNoteIndex != -1 && selectedNoteIndex < stageData.actions.Count)
        {
            var action = stageData.actions[selectedNoteIndex];

            EditorGUILayout.LabelField($"노트 설정 (Beat: {action.beat})", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            action.beat = EditorGUILayout.FloatField("Beat Position", action.beat);
            action.role = (ActionRole)EditorGUILayout.EnumPopup("Action Role", action.role);
            action.noteType = (NoteType)EditorGUILayout.EnumPopup("Note Type", action.noteType);
            action.type = (PatternType)EditorGUILayout.EnumPopup("Pattern Type", action.type);
            action.duration = EditorGUILayout.FloatField("Hold Duration", action.duration);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(stageData, "Edit Note");

                stageData.actions[selectedNoteIndex] = action;
                stageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));

                EditorUtility.SetDirty(stageData);
            }
        }
    }

    private void DrawNotes(Rect rect, float trackHeight)
    {
        if (stageData?.actions == null) return;
        for (int i = 0; i < stageData.actions.Count; i++)
        {
            var action = stageData.actions[i];
            float xPos = rect.x + (action.beat * PixelsPerBeat);
            Color noteColor = (action.role == ActionRole.Signal) ? Color.cyan : new Color(1f, 0.8f, 0f);
            if (i == selectedNoteIndex) noteColor = Color.white;
            float nHeight = trackHeight * 0.7f;
            float noteY = rect.y + (trackHeight - nHeight) * 0.5f;
            EditorGUI.DrawRect(new Rect(xPos - 8, noteY, 16, nHeight), noteColor);
            GUI.Label(new Rect(xPos - 10, noteY + nHeight + 2, 40, 20), action.beat.ToString("F1"), EditorStyles.miniLabel);
        }
    }

    private void OnStageDataChanged() { if (stageData == null) return; musicClip = stageData.masterTrack; if (previewSource != null) previewSource.clip = musicClip; bpm = stageData.bpm; }
    private void TogglePlay() { if (previewSource.isPlaying) previewSource.Pause(); else previewSource.Play(); Repaint(); }
    private void StopMusic() { previewSource.Stop(); previewSource.time = 0; scroll.x = 0; Repaint(); }
    private void HandleShortcuts() { if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space) { TogglePlay(); Event.current.Use(); } }

    private void DrawControlBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUI.BeginChangeCheck();
        stageData = (StageData)EditorGUILayout.ObjectField(stageData, typeof(StageData), false);
        if (EditorGUI.EndChangeCheck()) OnStageDataChanged();
        EditorGUILayout.Space(10);
        previewSource = (AudioSource)EditorGUILayout.ObjectField(previewSource, typeof(AudioSource), true);

        if (GUILayout.Button(previewSource != null && previewSource.isPlaying ? "멈춤" : "시작", EditorStyles.toolbarButton, GUILayout.Width(50))) TogglePlay();
        if (GUILayout.Button("정지", EditorStyles.toolbarButton, GUILayout.Width(40))) StopMusic();

        if (stageData != null)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Start", GUILayout.Width(35));
            float newStart = EditorGUILayout.FloatField(stageData.playStartTime, GUILayout.Width(40));

            EditorGUILayout.LabelField("End", GUILayout.Width(30));
            float newEnd = EditorGUILayout.FloatField(stageData.endPosition, GUILayout.Width(40));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(stageData, "Edit Stage Range");

                stageData.playStartTime = newStart;
                stageData.endPosition = newEnd;

                EditorUtility.SetDirty(stageData);
            }
        }
        EditorGUILayout.LabelField("BPM", GUILayout.Width(35));
        bpm = EditorGUILayout.FloatField(bpm, GUILayout.Width(40));
        EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
        pixelPerSecond = EditorGUILayout.FloatField(pixelPerSecond, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();
        HandleShortcuts();
    }

    private void DrawPlaybackRange(Rect rect)
    {
        if (stageData == null) return;
        float startX = rect.x + (stageData.playStartTime * pixelPerSecond);
        float endX = rect.x + (stageData.endPosition * pixelPerSecond);
        Rect rangeRect = new Rect(startX, rect.y, endX - startX, rect.height);
        EditorGUI.DrawRect(rangeRect, new Color(1f, 1f, 1f, 0.03f));
        Handles.color = Color.green; Handles.DrawLine(new Vector3(startX, rect.y), new Vector3(startX, rect.yMax), 2f);
        Handles.color = new Color(0.6f, 0.3f, 0.9f); Handles.DrawLine(new Vector3(endX, rect.y), new Vector3(endX, rect.yMax), 2f);
    }

    private void DrawGrid(Rect rect)
    {
        float beatInterval = 60f / bpm;
        int totalBeats = Mathf.FloorToInt(musicClip.length / beatInterval);
        for (int i = 0; i <= totalBeats; i++)
        {
            float xPos = rect.x + (i * beatInterval * pixelPerSecond);
            bool isBar = i % 4 == 0;
            Handles.color = isBar ? new Color(0.5f, 0.5f, 0.5f, 0.8f) : new Color(0.3f, 0.3f, 0.3f, 0.3f);
            Handles.DrawLine(new Vector3(xPos, rect.y), new Vector3(xPos, rect.yMax));
            if (isBar) GUI.Label(new Rect(xPos + 5, rect.y + 2, 50, 20), $"M {i / 4}", EditorStyles.boldLabel);
            else GUI.Label(new Rect(xPos + 5, rect.y + 18, 30, 20), $"{(i % 4) + 1}", EditorStyles.miniLabel);
        }
    }

    private void DrawPlayhead(Rect rect)
    {
        if (previewSource == null) return;
        float xPos = rect.x + (previewSource.time * pixelPerSecond);
        Handles.color = Color.red; Handles.DrawLine(new Vector3(xPos, rect.y), new Vector3(xPos, rect.yMax), 2f);
    }

    private void HandleInput(Rect rect, float noteTrackHeight)
    {
        Event e = Event.current;
        if (!rect.Contains(e.mousePosition)) return;

        float relativeX = e.mousePosition.x - rect.x;
        float relativeY = e.mousePosition.y - rect.y;
        float snappedBeat = Mathf.Round((relativeX / PixelsPerBeat) * 2f) / 2f;

        bool isThemeLane = relativeY > noteTrackHeight;

        if (e.type == EventType.MouseDown)
        {
            if (e.button == 0)
            {
                if (isThemeLane)
                {
                    selectedThemeEventIndex = stageData.themeEvents.FindIndex(t => Mathf.Approximately(t.beat, snappedBeat));
                    selectedNoteIndex = -1;
                }
                else
                {
                    selectedNoteIndex = stageData.actions.FindIndex(a => Mathf.Abs(a.beat - snappedBeat) < 0.05f);
                    selectedThemeEventIndex = -1;
                }

                if (selectedNoteIndex == -1 && selectedThemeEventIndex == -1 && previewSource != null)
                    previewSource.time = Mathf.Clamp(relativeX / pixelPerSecond, 0, musicClip.length - 0.01f);

                Repaint();
                e.Use();
            }
            else if (e.button == 1)
            {
                // ⭐ 핵심 Undo
                Undo.RecordObject(stageData, "Modify Rhythm Data");

                if (isThemeLane)
                {
                    int idx = stageData.themeEvents.FindIndex(t => Mathf.Approximately(t.beat, snappedBeat));

                    if (idx != -1)
                        stageData.themeEvents.RemoveAt(idx);
                    else
                        stageData.themeEvents.Add(new ThemeEvent { beat = snappedBeat, theme = StageThemeType.Stage1 });

                    stageData.themeEvents.Sort((a, b) => a.beat.CompareTo(b.beat));
                }
                else
                {
                    int idx = stageData.actions.FindIndex(a => Mathf.Approximately(a.beat, snappedBeat));

                    if (idx != -1)
                        stageData.actions.RemoveAt(idx);
                    else
                        stageData.actions.Add(new RhythmAction
                        {
                            beat = snappedBeat,
                            noteType = NoteType.Runtime,
                            role = ActionRole.Hit
                        });

                    stageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));
                }

                EditorUtility.SetDirty(stageData);
                Repaint();
                e.Use();
            }
        }
    }

    private void DrawGuide()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("좌: 선택/이동 | 우: 생성/삭제 | Space: 재생", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Track: 상단(Rhythm Action) / 하단(Theme Event)", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }
}