using System.Collections.Generic;
using UnityEngine;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Data.Enum;
using Project.Core.Managers;

namespace Project.Rhythm.Editor
{
    public class BeatRecorder : MonoBehaviour
    {
        [SerializeField] private KeyCode tapKey = KeyCode.Space;
        [SerializeField] private KeyCode slideKey = KeyCode.A;
        [SerializeField] private KeyCode holdKey = KeyCode.D;
        [SerializeField] private KeyCode saveKey = KeyCode.S;

        [SerializeField] private bool useSnapping = true;
        [Range(1, 16)][SerializeField] private int snapDivision = 4;

        [SerializeField] private StageData targetStageData;

        private readonly List<RhythmAction> _recordedActions = new();
        private float _secondsPerBeat;

        // 홀드 기록용 임시 변수
        private float? _holdStartBeat = null;

        private void Start()
        {
            if (targetStageData == null)
            {
                Debug.LogError("<color=red>[Recorder]</color> Target Stage Data가 없습니다!");
                return;
            }

            _secondsPerBeat = 60f / targetStageData.bpm;
            Debug.Log($"<color=cyan>[Recorder]</color> 시작 | BPM: {targetStageData.bpm}");
        }

        private void Update()
        {
            if (targetStageData == null) return;

            if (Input.GetKeyDown(tapKey)) TryRecord(PatternType.Tap);
            if (Input.GetKeyDown(slideKey)) TryRecord(PatternType.Slide);

            if (Input.GetKeyDown(holdKey))
            {
                _holdStartBeat = GetCurrentBeat();
                Debug.Log($"<color=orange>[Hold Start]</color> Beat: {_holdStartBeat:F2}");
            }

            if (Input.GetKeyUp(holdKey) && _holdStartBeat.HasValue)
            {
                float endBeat = GetCurrentBeat();
                float duration = endBeat - _holdStartBeat.Value;

                if (duration < 0.1f) duration = 1f / snapDivision;

                _recordedActions.Add(new RhythmAction
                {
                    beat = _holdStartBeat.Value,
                    type = PatternType.Hold,
                    duration = duration
                });

                Debug.Log($"<color=orange>[Hold End]</color> Beat: {_holdStartBeat:F2} | Duration: {duration:F2}");
                _holdStartBeat = null;
            }

            // 4. 데이터 저장
            if (Input.GetKeyDown(saveKey)) SaveToStageData();
        }

        private float GetCurrentBeat()
        {
            float currentTime = StageManager.CurrentTime;
            if (currentTime <= 0f) return 0f;

            float rawBeat = currentTime / _secondsPerBeat;
            return useSnapping ? Mathf.Round(rawBeat * snapDivision) / snapDivision : rawBeat;
        }

        private void TryRecord(PatternType type)
        {
            float finalBeat = GetCurrentBeat();
            if (StageManager.CurrentTime <= 0f) return;

            _recordedActions.Add(new RhythmAction
            {
                beat = finalBeat,
                type = type,
                duration = 0f
            });

            Debug.Log($"<color=yellow>[Recorded]</color> {type} | Beat: {finalBeat:F2}");
        }

        private void SaveToStageData()
        {
            if (targetStageData == null || _recordedActions.Count == 0) return;

            foreach (var action in _recordedActions)
            {
                targetStageData.actions.Add(action);
            }

            targetStageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(targetStageData);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            Debug.Log($"<color=green>[Saved]</color> {targetStageData.name}에 {_recordedActions.Count}개 노트 저장 완료!");
            _recordedActions.Clear();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || targetStageData == null) return;

            GUIStyle style = new GUIStyle { fontSize = 20, normal = { textColor = Color.cyan } };
            GUIStyle boldStyle = new GUIStyle { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow } };
            GUIStyle recordingStyle = new GUIStyle { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow } };

            GUILayout.BeginArea(new Rect(30, 30, 600, 500));
            GUILayout.Label($"Recording for: {targetStageData.name}", style);
            GUILayout.Space(10);

            GUILayout.Label($"[ {tapKey} ] : TAP (Bat/Normal)", style);
            GUILayout.Label($"[ {slideKey} ] : SLIDE", style);
            GUILayout.Label($"[ {holdKey} ] : HOLD (Stone - Press & Release)", style);
            GUILayout.Space(15);

            float currentBeat = StageManager.CurrentTime / _secondsPerBeat;
            GUILayout.Label($"Current Beat: {currentBeat:F2}", boldStyle);

            if (_holdStartBeat.HasValue)
            {
                float currentDuration = currentBeat - _holdStartBeat.Value;
                GUILayout.Label($"● HOLDING... Duration: {currentDuration:F2}", recordingStyle);
            }

            GUILayout.Label($"Recorded Count: {_recordedActions.Count}", style);
            GUILayout.Space(10);
            GUILayout.Label($"Press '{saveKey}' to Save to Asset", boldStyle);
            GUILayout.EndArea();
        }
    }
}