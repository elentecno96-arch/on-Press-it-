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
        [Header("Keys")]
        [SerializeField] private KeyCode tapKey = KeyCode.Space;
        [SerializeField] private KeyCode slideKey = KeyCode.A;
        [SerializeField] private KeyCode holdKey = KeyCode.D;
        [SerializeField] private KeyCode signalModifierKey = KeyCode.LeftShift; // 시그널 모드 키
        [SerializeField] private KeyCode saveKey = KeyCode.S;

        [Header("Settings")]
        [SerializeField] private bool useSnapping = true;
        [Range(1, 16)][SerializeField] private int snapDivision = 4;
        [SerializeField] private string currentTargetID = "Stage3_Stone"; // 현재 기록 중인 타겟 ID

        [SerializeField] private StageData targetStageData;

        private readonly List<RhythmAction> _recordedActions = new();
        private float _secondsPerBeat;
        private float? _holdStartBeat = null;

        private void Start()
        {
            if (targetStageData == null) return;
            _secondsPerBeat = 60f / targetStageData.bpm;
        }

        private void Update()
        {
            if (targetStageData == null) return;

            // 현재 시그널 모드인지 확인
            bool isSignalMode = Input.GetKey(signalModifierKey);

            if (Input.GetKeyDown(tapKey)) TryRecord(PatternType.Tap, isSignalMode);
            if (Input.GetKeyDown(slideKey)) TryRecord(PatternType.Slide, isSignalMode);

            // 홀드 로직 (홀드도 시그널로 찍을 수 있음)
            if (Input.GetKeyDown(holdKey))
            {
                _holdStartBeat = GetCurrentBeat();
            }

            if (Input.GetKeyUp(holdKey) && _holdStartBeat.HasValue)
            {
                RecordHold(isSignalMode);
            }

            if (Input.GetKeyDown(saveKey)) SaveToStageData();
        }

        private void TryRecord(PatternType type, bool isSignal)
        {
            if (StageManager.CurrentTime <= 0f) return;

            _recordedActions.Add(new RhythmAction
            {
                beat = GetCurrentBeat(),
                type = type,
                role = isSignal ? ActionRole.Signal : ActionRole.Hit, // 역할 기록
                targetID = isSignal || type == PatternType.Hold ? currentTargetID : "", // 필요한 경우 ID 부여
                duration = 0f
            });

            string roleText = isSignal ? "<color=cyan>[SIGNAL]</color>" : "<color=yellow>[HIT]</color>";
            Debug.Log($"{roleText} {type} 기록됨");
        }

        private void RecordHold(bool isSignal)
        {
            float endBeat = GetCurrentBeat();
            float duration = Mathf.Max(endBeat - _holdStartBeat.Value, 1f / snapDivision);

            _recordedActions.Add(new RhythmAction
            {
                beat = _holdStartBeat.Value,
                type = PatternType.Hold,
                role = isSignal ? ActionRole.Signal : ActionRole.Hit,
                targetID = currentTargetID,
                duration = duration
            });
            _holdStartBeat = null;
        }

        private float GetCurrentBeat()
        {
            float currentTime = StageManager.CurrentTime;
            if (currentTime <= 0f) return 0f;
            float rawBeat = currentTime / _secondsPerBeat;
            return useSnapping ? Mathf.Round(rawBeat * snapDivision) / snapDivision : rawBeat;
        }

        private void SaveToStageData()
        {
            if (targetStageData == null || _recordedActions.Count == 0) return;
            targetStageData.actions.AddRange(_recordedActions);
            targetStageData.actions.Sort((a, b) => a.beat.CompareTo(b.beat));

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(targetStageData);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            _recordedActions.Clear();
            Debug.Log("<color=green>저장 완료!</color>");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || targetStageData == null) return;

            GUIStyle style = new GUIStyle { fontSize = 20, normal = { textColor = Color.white } };
            bool isSignalMode = Input.GetKey(signalModifierKey);

            GUILayout.BeginArea(new Rect(30, 30, 600, 600));

            // 시그널 모드 강조 표시
            string modeText = isSignalMode ? "<color=cyan>MODE: SIGNAL RECORDING</color>" : "<color=yellow>MODE: HIT RECORDING</color>";
            GUILayout.Label(modeText, new GUIStyle(style) { fontSize = 25, fontStyle = FontStyle.Bold });

            GUILayout.Label($"Recording for: {targetStageData.name}", style);
            GUILayout.Label($"Modifier Key [{signalModifierKey}] : Hold to record Signals", style);
            GUILayout.Space(10);

            GUILayout.Label($"Target ID: {currentTargetID}", style);
            GUILayout.Label($"Recorded Count: {_recordedActions.Count}", style);

            GUILayout.EndArea();
        }
    }
}