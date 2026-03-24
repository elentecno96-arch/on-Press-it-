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
            public float duration;          //개별 노트 유지 시간
        }

        private readonly List<EventData> _events = new();
        private int _currentIndex;
        private float _secondsPerBeat;
        private int _nextAutoCountIndex; // 카운트다운용 인덱스 별도 관리
        private const float BEAT_LEAD_TIME = 2.0f; 
        private float _lastTriggeredBeat = -1f;

        public event Action<float> OnCountdownTriggered;
        public event Action<RhythmAction, float, float> OnSpawnTriggered;

        public void Initialize(StageData data, float defaultAppearDuration)
        {
            _events.Clear();
            _currentIndex = 0;
            _nextAutoCountIndex = 0;
            _lastTriggeredBeat = -1f;

            if (data == null || data.bpm <= 0) return;

            _secondsPerBeat = 60f / data.bpm;

            foreach (var action in data.actions)
            {
                if (action.type == PatternType.None) continue;

                float hitTime = action.beat * _secondsPerBeat;
                float duration = (action.type == PatternType.Hold) ? 0.1f : defaultAppearDuration;
                float spawnTime = hitTime - duration;

                _events.Add(new EventData
                {
                    action = action,
                    spawnTriggerTime = spawnTime,
                    targetHitTime = hitTime,
                    duration = duration
                });
            }

            _events.Sort((a, b) => a.spawnTriggerTime.CompareTo(b.spawnTriggerTime));
        }

        /// <summary>
        /// StageManager의 Update에서 호출되어 시간을 체크함
        /// </summary>
        public void Process(float stageTime)
        {
            if (stageTime < 0) return;

            if (_nextAutoCountIndex < _events.Count)
            {
                var evt = _events[_nextAutoCountIndex];

                if (evt.action.type == PatternType.Hold && evt.action.role == ActionRole.Hit)
                {
                    float countdownStartTime = evt.targetHitTime - (BEAT_LEAD_TIME * _secondsPerBeat);

                    if (stageTime >= countdownStartTime && _lastTriggeredBeat < evt.action.beat)
                    {
                        OnCountdownTriggered?.Invoke(evt.action.beat);
                        _lastTriggeredBeat = evt.action.beat;
                        _nextAutoCountIndex++;
                    }
                }
                else
                {
                    _nextAutoCountIndex++;
                }
            }
            while (_currentIndex < _events.Count)
            {
                EventData evt = _events[_currentIndex];
                if (stageTime < evt.spawnTriggerTime) break;

                OnSpawnTriggered?.Invoke(evt.action, evt.targetHitTime, evt.duration);
                _currentIndex++;
            }
        }

        public void Reset()
        {
            _currentIndex = 0;
            _nextAutoCountIndex = 0;
            _lastTriggeredBeat = -1f;
        }
    }
}