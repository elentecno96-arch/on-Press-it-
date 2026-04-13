using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Rhythm.Interface;
using System;
using System.Collections.Generic;

namespace Project.Rhythm.Event
{
    /// <summary>
    /// StageTime을 기준으로 노트를 생성해야 할 시점을 트리거하는 시스템.
    /// </summary>
    public class RhythmEventSystem
    {
        private struct EventData
        {
            public RhythmAction action;
            public float spawnTriggerTime; // 실제 소환되어야 하는 절대 시간
            public float targetHitTime;    // 정박(판정) 절대 시간
            public float duration;          //개별 노트 유지 시간
        }

        private readonly List<EventData> _events = new();
        private readonly ICurrentTime _timeProvider;
        private int _currentIndex;
        private float _secondsPerBeat;

        public event Action<RhythmAction, float, float> OnSpawnTriggered;

        public RhythmEventSystem(ICurrentTime timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public void Initialize(StageData data, float defaultAppearDuration)
        {
            _events.Clear();
            _currentIndex = 0;

            if (data == null || data.bpm <= 0) return;

            _secondsPerBeat = 60f / data.bpm;

            if (data.actions != null) _events.Capacity = data.actions.Count;

            foreach (var action in data.actions)
            {
                if (action.type == PatternType.None) continue;

                float hitTime = action.beat * _secondsPerBeat;
                float duration = (action.type == PatternType.Hold)
                                 ? action.duration * _secondsPerBeat
                                 : defaultAppearDuration;

                _events.Add(new EventData
                {
                    action = action,
                    spawnTriggerTime = hitTime - duration,
                    targetHitTime = hitTime,
                    duration = duration
                });
            }

            _events.Sort((a, b) => a.spawnTriggerTime.CompareTo(b.spawnTriggerTime));
        }

        /// <summary>
        /// StageManager의 Update에서 호출되어 시간을 체크함
        /// </summary>
        public void Process()
        {
            float stageTime = _timeProvider.CurrentTime;
            if (stageTime < 0) return;

            while (_currentIndex < _events.Count)
            {
                EventData evt = _events[_currentIndex];

                if (stageTime < evt.spawnTriggerTime) break;

                OnSpawnTriggered?.Invoke(evt.action, evt.targetHitTime, evt.duration);
                _currentIndex++;
            }
        }

        public void SyncToTime(float stageTime)
        {
            _currentIndex = 0;

            while (_currentIndex < _events.Count && _events[_currentIndex].targetHitTime < stageTime)
            {
                _currentIndex++;
            }

            int tempSpawnIndex = _currentIndex;
            while (tempSpawnIndex < _events.Count)
            {
                var evt = _events[tempSpawnIndex];

                if (evt.spawnTriggerTime <= stageTime && evt.targetHitTime > stageTime)
                {
                    OnSpawnTriggered?.Invoke(evt.action, evt.targetHitTime, evt.duration);
                    _currentIndex = tempSpawnIndex + 1;
                }

                tempSpawnIndex++;

                if (tempSpawnIndex < _events.Count && _events[tempSpawnIndex].spawnTriggerTime > stageTime + 2.0f)
                    break;
            }
        }
    }
}