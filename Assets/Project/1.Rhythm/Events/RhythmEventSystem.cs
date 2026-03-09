using Project.Rhythm.Data;
using System;
using System.Collections.Generic;

namespace Project.Rhythm.Event
{
    /// <summary>
    /// StageTime을 기준으로 노트를 생성해야 할 시점을 트리거하는 시스템
    /// </summary>
    public class RhythmEventSystem
    {
        private struct EventData
        {
            public RhythmAction action;
            public float spawnTriggerTime; // 실제 생성되어야 하는 StageTime
            public float targetHitTime;    // 판정 지점 도착 StageTime
        }

        private readonly List<EventData> _events = new();
        private int _currentIndex;
        private float _secondsPerBeat;

        public event Action<RhythmAction, float> OnSpawnTriggered;

        public void Initialize(StageData data, float appearDuration)
        {
            _events.Clear();
            _currentIndex = 0;
            _secondsPerBeat = 60f / data.bpm;

            foreach (var pattern in data.patterns)
            {
                foreach (var action in pattern.actions)
                {
                    float hitTime = action.beat * _secondsPerBeat;

                    float spawnTime = hitTime - (appearDuration * 0.7f);

                    _events.Add(new EventData
                    {
                        action = action,
                        spawnTriggerTime = spawnTime,
                        targetHitTime = hitTime
                    });
                }
            }
            _events.Sort((a, b) => a.spawnTriggerTime.CompareTo(b.spawnTriggerTime));
        }

        /// <summary>
        /// StageManager에서 계산된 stageTime(musicTime - playStartTime)을 인자로 받습니다
        /// </summary>
        public void Process(float stageTime)
        {
            if (stageTime < 0f) return;

            while (_currentIndex < _events.Count)
            {
                var evt = _events[_currentIndex];

                if (stageTime >= evt.spawnTriggerTime)
                {
                    OnSpawnTriggered?.Invoke(evt.action, evt.targetHitTime);
                    _currentIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        public void Reset()
        {
            _currentIndex = 0;
        }
    }
}