using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Rhythm.Interface;
using System;
using System.Collections.Generic;

namespace Project.Rhythm.Event
{
    /// <summary>
    /// 카운트 다운 시스템
    /// 이벤트 시스템에서 분리 됨
    /// </summary>
    public class CountdownSystem
    {
        private readonly ICurrentTime _timeProvider;
        private readonly List<CountdownData> _countdownEvents = new();
        private int _nextIndex;
        private float _secondsPerBeat;
        private float _lastTriggeredBeat = -1f;
        private const float BEAT_LEAD_TIME = 2f;

        public event Action<float> OnCountdownTriggered;

        private struct CountdownData
        {
            public float beat;
            public float triggerTime;
        }

        public CountdownSystem(ICurrentTime timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public void Initialize(StageData data)
        {
            _countdownEvents.Clear();
            _nextIndex = 0;
            _lastTriggeredBeat = -1f;

            if (data == null || data.bpm <= 0) return;
            _secondsPerBeat = 60f / data.bpm;

            foreach (var action in data.actions)
            {
                if (action.type == PatternType.Hold && action.role == ActionRole.Hit)
                {
                    float hitTime = action.beat * _secondsPerBeat;
                    _countdownEvents.Add(new CountdownData
                    {
                        beat = action.beat,
                        triggerTime = hitTime - (BEAT_LEAD_TIME * _secondsPerBeat)
                    });
                }
            }
            _countdownEvents.Sort((a, b) => a.triggerTime.CompareTo(b.triggerTime));
        }

        public void Process()
        {
            float stageTime = _timeProvider.CurrentTime;
            if (stageTime < 0) return;

            while (_nextIndex < _countdownEvents.Count)
            {
                var data = _countdownEvents[_nextIndex];
                if (stageTime < data.triggerTime) break;

                if (_lastTriggeredBeat < data.beat)
                {
                    OnCountdownTriggered?.Invoke(data.beat);
                    _lastTriggeredBeat = data.beat;
                }
                _nextIndex++;
            }
        }

        public void SyncToTime(float stageTime)
        {
            _nextIndex = 0;
            _lastTriggeredBeat = -1f;
            while (_nextIndex < _countdownEvents.Count && _countdownEvents[_nextIndex].triggerTime < stageTime)
            {
                _nextIndex++;
            }
        }
    }
}
