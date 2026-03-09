using System.Collections.Generic;
using UnityEngine;
using Project.Rhythm.Data;
using Project.Core.Managers;

namespace Project.Rhythm.Editor
{
    public class BeatRecorder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode recordKey = KeyCode.Space;
        [SerializeField] private float bpm = 120f;
        [SerializeField] private bool useSnapping = true; 

        [Header("Target Data")]
        [SerializeField] private PatternData targetPatternData;

        private List<RhythmAction> _recordedActions = new List<RhythmAction>();
        private float _secondsPerBeat;

        private void Start()
        {
            _secondsPerBeat = 60f / bpm;
            Debug.Log($"<color=cyan>[Recorder]</color> 초기화 완료. BPM: {bpm}, 1비트당 시간: {_secondsPerBeat}s");
        }

        private void Update()
        {
            if (Input.GetKeyDown(recordKey))
            {
                Debug.Log($"<color=white>[Input Check]</color> Space Key Pressed! CurrentTime: {StageManager.CurrentTime}");
        
        if (StageManager.CurrentTime <= 0)
                {
                    Debug.LogWarning("<color=red>[Warning]</color> StageManager.CurrentTime이 0 이하입니다. 음악이 재생 중인지 확인하세요!");
            return;
                }

                RecordHit();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveToSO();
            }
        }

        private void RecordHit()
        {
            float rawBeat = StageManager.CurrentTime / _secondsPerBeat;
            float finalBeat = rawBeat;

            if (useSnapping)
            {
                finalBeat = Mathf.Round(rawBeat * 4f) / 4f;
            }

            var newAction = new RhythmAction
            {
                beat = finalBeat,
                type = PatternType.Tap,
                spawnStrategyId = "Center",
                moveStrategyId = "Scale",
                actionStrategyId = "Tap"
            };

            _recordedActions.Add(newAction);

            Debug.Log($"<color=yellow>[Record]</color> No.{_recordedActions.Count} | " +
                      $"Beat: {finalBeat:F2} (Raw: {rawBeat:F2}) | " +
                      $"Time: {StageManager.CurrentTime:F2}s"); 
        }

        private void SaveToSO()
        {
            if (targetPatternData == null)
            {
                Debug.LogError("<color=red>[Recorder]</color> 대상 PatternData SO가 할당되지 않았습니다!");
                return;
            }

            if (_recordedActions.Count == 0)
            {
                Debug.LogWarning("<color=orange>[Recorder]</color> 기록된 데이터가 없어 저장을 건너뜁니다.");
                return;
            }
            // targetPatternData.actions.Clear(); 

            foreach (var action in _recordedActions)
            {
                targetPatternData.actions.Add(action);
            }

            targetPatternData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(targetPatternData);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"<color=green>[Success]</color> {targetPatternData.name}에 {_recordedActions.Count}개 저장 완료!");
            _recordedActions.Clear();
        }

        private void OnGUI()
        {
            if (StageManager.CurrentTime <= 0) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 25;
            style.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(20, 20, 400, 200));
            GUILayout.Label($"Recording Key: {recordKey}", style); 
            GUILayout.Label($"Current Beat: {(StageManager.CurrentTime / _secondsPerBeat):F2}", style); 
            GUILayout.Label($"Recorded Count: {_recordedActions.Count}", style);
            GUILayout.Label("Press 'S' to Save", style);
            GUILayout.EndArea();
        }
    }
}