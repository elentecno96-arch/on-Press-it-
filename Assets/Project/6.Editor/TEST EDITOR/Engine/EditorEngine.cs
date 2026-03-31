using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.TestEditor.Engine
{
    /// <summary>
    /// 에디터의 내부의 데이터 흐름을 담당하는 최상위 클래스
    /// </summary>
    public class EditorEngine
    {
        public StageData currentStageData;

        public AudioSource audioSource; 
        public float masterVolume = 1.0f;

        public float currentTime = 0f;
        public float scrollX = 0f;
        public float zoomLevel = 100f;
        public bool isPlaying = false;
        private double _lastTime;

        public float TotalDuration => (masterTrack != null) ? masterTrack.length : 300f;
        public float TotalPixelWidth => TotalDuration * zoomLevel;

        public int stageIndex;
        public bool skipGuide;
        public string stageName;
        public AudioClip masterTrack;
        public float bpm;

        public float perfectWindow = 0.12f;
        public float greatWindow = 0.21f;
        public float goodWindow = 0.27f;
        public float missWindow = 0.34f;

        public List<ThemeResource> themeResources = new();

        public List<RhythmAction> actions = new();
        public List<ThemeEvent> themeEvents = new();

        public int selectedActionIndex = -1;
        public int selectedThemeEventIndex = -1;

        public void OnUpdate()
        {
            if (currentStageData == null) return;

            double now = UnityEditor.EditorApplication.timeSinceStartup;
            double deltaTime = now - _lastTime;
            _lastTime = now;

            if (isPlaying)
            {
                currentTime += (float)deltaTime;

                if (audioSource != null && masterTrack != null)
                {
                    if (audioSource.clip != masterTrack)
                    {
                        audioSource.clip = masterTrack;
                    }

                    if (!audioSource.isPlaying)
                    {
                        audioSource.time = Mathf.Clamp(currentTime, 0, TotalDuration - 0.1f);
                        audioSource.Play();
                    }

                    if (Mathf.Abs(audioSource.time - currentTime) > 0.1f)
                    {
                        audioSource.time = currentTime;
                    }
                }

                float playheadPixelX = currentTime * zoomLevel;
                float viewWidth = 710f;
                float centerThreshold = scrollX + (viewWidth * 0.5f);

                if (playheadPixelX > centerThreshold)
                {
                    scrollX = playheadPixelX - (viewWidth * 0.5f);
                }

                if (currentTime >= TotalDuration)
                {
                    currentTime = TotalDuration;
                    isPlaying = false;
                }
            }
            else
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
            }

            float maxScroll = Mathf.Max(0, TotalPixelWidth - 710f);
            scrollX = Mathf.Clamp(scrollX, 0, maxScroll);
        }

        public void ClearSelection()
        {
            selectedActionIndex = -1;
            selectedThemeEventIndex = -1;
        }

        /// <summary>
        /// 현재 에디터의 설정값을 할당된 StageData SO에 덮어씌웁니다.
        /// </summary>
        public void SaveToSO()
        {
            if (currentStageData == null)
            {
                Debug.LogWarning("대상 StageData가 할당되지 않아 저장할 수 없습니다.");
                return;
            }

            currentStageData.stageIndex = stageIndex;
            currentStageData.stageName = stageName;
            currentStageData.masterTrack = masterTrack;
            currentStageData.bpm = bpm;

            currentStageData.actions = new List<RhythmAction>(actions);
            currentStageData.themeEvents = new List<ThemeEvent>(themeEvents);

            EditorUtility.SetDirty(currentStageData);
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[SO Save]</color> {currentStageData.name} 저장 완료!");
        }

        public void LoadFromSO()
        {
            if (currentStageData == null)
            {
                Debug.LogWarning("불러올 StageData SO가 할당되지 않았습니다.");
                return;
            }
            this.stageIndex = currentStageData.stageIndex;
            this.stageName = currentStageData.stageName;
            this.masterTrack = currentStageData.masterTrack;
            this.bpm = currentStageData.bpm;

            this.perfectWindow = currentStageData.perfectWindow;
            this.greatWindow = currentStageData.greatWindow;
            this.goodWindow = currentStageData.goodWindow;
            this.missWindow = currentStageData.missWindow;

            this.actions = new List<RhythmAction>(currentStageData.actions);
            this.themeEvents = new List<ThemeEvent>(currentStageData.themeEvents);
            this.themeResources = new List<ThemeResource>(currentStageData.themeResources);

            this.currentTime = 0f;
            this.scrollX = 0f;
            this.isPlaying = false;
            ClearSelection();

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.time = 0f;
            }

            Debug.Log($"<color=yellow>[SO Load]</color> {currentStageData.name} 데이터를 성공적으로 불러왔습니다.");
        }

        /// <summary>
        /// JSON으로 저장하는 로직 (기능 정의만)
        /// </summary>
        public void SaveToJson()
        {
            if (currentStageData == null) return;

            var saveData = new StageSaveData
            {
                stageName = this.stageName,
                bpm = this.bpm,
                actions = this.actions,
                themeEvents = this.themeEvents
            };

            string json = JsonUtility.ToJson(saveData, true);
            string path = EditorUtility.SaveFilePanel("Save Stage JSON", "Assets", stageName, "json");

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                Debug.Log($"<color=cyan>[JSON Save]</color> {path}에 저장되었습니다.");
            }
        }

        /// <summary>
        /// JSON 불러오기 로직 (기능 정의만)
        /// </summary>
        public void LoadFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Load Stage JSON", "Assets", "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = File.ReadAllText(path);
                StageSaveData loadedData = JsonUtility.FromJson<StageSaveData>(json);

                // 엔진 데이터에 덮어쓰기
                this.stageName = loadedData.stageName;
                this.bpm = loadedData.bpm;
                this.actions = new List<RhythmAction>(loadedData.actions);
                this.themeEvents = new List<ThemeEvent>(loadedData.themeEvents);

                Debug.Log("<color=yellow>[JSON Load]</color> 데이터를 성공적으로 불러왔습니다.");
            }
        }

        public float ScreenToTime(float mouseX, Rect timelineRect)
        {
            float relativeX = mouseX - timelineRect.x + scrollX;
            return relativeX / zoomLevel;
        }

        public float TimeToBeat(float time)
        {
            if (bpm <= 0) return 0;
            return (time * bpm) / 60f;
        }

        public float BeatToTime(float beat)
        {
            if (bpm <= 0) return 0;
            return (beat * 60f) / bpm;
        }

        public void AddNote(float time, int lane)
        {
            float beatValue = TimeToBeat(time);

            if (actions.Exists(a => Mathf.Approximately(a.beat, beatValue)))
            {
                return;
            }

            RhythmAction newAction = new RhythmAction
            {
                beat = beatValue,
                type = PatternType.Tap,
                role = ActionRole.Hit,
                noteType = NoteType.Runtime
            };

            actions.Add(newAction);
            Debug.Log($"<color=cyan>[Note Added]</color> Beat: {beatValue}, Lane: {lane}");
        }

        public void AddThemeEvent(float time)
        {
            float snappedTime = GetSnappedTime(time);
            float beatValue = TimeToBeat(snappedTime);

            if (themeEvents.Exists(t => Mathf.Abs(t.beat - beatValue) < 0.01f)) return;

            themeEvents.Add(new ThemeEvent
            {
                beat = beatValue,
                theme = StageThemeType.Stage1
            });
            themeEvents.Sort((a, b) => a.beat.CompareTo(b.beat));
        }

        public void RemoveNote(float time)
        {
            float snappedTime = GetSnappedTime(time);
            float beatValue = TimeToBeat(snappedTime);

            int index = actions.FindIndex(a => Mathf.Abs(a.beat - beatValue) < 0.01f);

            if (index != -1)
            {
                actions.RemoveAt(index);
                Debug.Log($"<color=red>[Note Removed]</color> Beat: {beatValue}");
            }
        }

        public void RemoveThemeEvent(float time)
        {
            float snappedTime = GetSnappedTime(time);

            float beatValue = TimeToBeat(snappedTime);

            int initialCount = themeEvents.Count;
            themeEvents.RemoveAll(t => Mathf.Abs(t.beat - beatValue) < 0.01f);

            if (themeEvents.Count < initialCount)
            {
                Debug.Log($"<color=red>[Theme Removed]</color> Beat: {beatValue:F2}");
            }
        }

        public float GetSnappedTime(float rawTime)
        {
            float beatDuration = 60f / bpm;
            // 4분음표 스냅 (나중에 8, 16분음표 선택 가능하게 확장 가능)
            float snapUnit = beatDuration;
            return Mathf.Round(rawTime / snapUnit) * snapUnit;
        }

        public void ClearData()
        {
            currentStageData = null;

            stageIndex = 0;
            stageName = string.Empty;
            masterTrack = null;
            bpm = 120f;
            currentTime = 0f;
            isPlaying = false;

            themeResources = new List<ThemeResource>();
            themeEvents = new List<ThemeEvent>();

            perfectWindow = 0.12f;
            greatWindow = 0.21f;
            goodWindow = 0.27f;
            missWindow = 0.34f;

            Debug.Log("<color=yellow>[Engine]</color> 모든 에디터 데이터가 초기화되었습니다.");
        }
    }
}