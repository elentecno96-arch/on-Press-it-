using Project.Rhythm.Data;
using System;
using System.Collections.Generic;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Data.Enum;

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
        }

        private readonly List<EventData> _events = new();
        private int _currentIndex;
        private float _secondsPerBeat;

        public event Action<RhythmAction, float> OnSpawnTriggered;

        public void Initialize(StageData data, float appearDuration)
        {
            _events.Clear();
            _currentIndex = 0;

            if (data == null || data.bpm <= 0) return;

            _secondsPerBeat = 60f / data.bpm;

            foreach (var action in data.actions)
            {
                if (action.type == PatternType.None)
                    continue;

                float hitTime = action.beat * _secondsPerBeat;

                float spawnTime = hitTime - appearDuration;

                _events.Add(new EventData
                {
                    action = action,
                    spawnTriggerTime = spawnTime,
                    targetHitTime = hitTime
                });
            }
            _events.Sort((a, b) => a.spawnTriggerTime.CompareTo(b.spawnTriggerTime));
        }

        /// <summary>
        /// StageManager의 Update에서 호출되어 시간을 체크함
        /// </summary>
        public void Process(float stageTime)
        {
            if (_currentIndex >= _events.Count)
                return;

            while (_currentIndex < _events.Count)
            {
                EventData evt = _events[_currentIndex];

                if (stageTime < evt.spawnTriggerTime)
                    break;
                OnSpawnTriggered?.Invoke(evt.action, evt.targetHitTime);

                _currentIndex++;
            }
        }

        public void Reset()
        {
            _currentIndex = 0;
        }
    }
}