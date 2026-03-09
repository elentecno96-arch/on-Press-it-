using Project.Rhythm.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm
{
    /// <summary>
    /// 리듬 이벤트 시스템 (순수클래스)
    /// </summary>
    public class RhythmEventSystem
    {
        private List<RhythmAction> _sortedActions;
        private int _currentIndex;
        private float _bpm;

        public void Initialize(StageData data)
        {
            _bpm = data.bpm;
            _sortedActions = new List<RhythmAction>();

            foreach (var p in data.patterns)
            {
                _sortedActions.AddRange(p.actions);
            }
            _sortedActions.Sort((a, b) => a.beat.CompareTo(b.beat));

            _currentIndex = 0;
        }

        public void Process(float currentTime)
        {
            if (_currentIndex >= _sortedActions.Count) return;

            // 박자를 시간으로 변환
            float targetTime = (_sortedActions[_currentIndex].beat * (60f / _bpm));

            if (currentTime >= targetTime)
            {
                TriggerAction(_sortedActions[_currentIndex]);
                _currentIndex++;
            }
        }

        private void TriggerAction(RhythmAction action)
        {
            Debug.Log($"[Event] {action.type} 실행! (Beat: {action.beat})");
            // 판정 시스템
        }
    }
}

