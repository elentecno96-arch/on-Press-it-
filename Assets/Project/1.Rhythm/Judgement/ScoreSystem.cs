using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Judgement
{
    /// <summary>
    /// 판정 결과를 받아 점수를 누적하고 최종 점수를 계산하는 역할
    /// 판정 시스템에서 분리됨
    /// </summary>
    public class ScoreSystem
    {
        private readonly Dictionary<JudgeResult, int> _judgeCounts = new();
        private int _totalNotes;

        public void Initialize(int totalNotes)
        {
            _totalNotes = totalNotes;
            _judgeCounts.Clear();
            foreach (JudgeResult res in System.Enum.GetValues(typeof(JudgeResult)))
                _judgeCounts[res] = 0;
        }

        public void AddResult(JudgeResult result)
        {
            if (_judgeCounts.TryGetValue(result, out int currentCount))
            {
                _judgeCounts[result] = currentCount + 1;
            }
        }

        public float CalculateScore()
        {
            if (_totalNotes <= 0) return 0f;

            const float PERFECT_WEIGHT = 1.0f;
            const float GREAT_WEIGHT = 0.7f;
            const float GOOD_WEIGHT = 0.4f;

            float weightedSum = (_judgeCounts[JudgeResult.Perfect] * PERFECT_WEIGHT) +
                               (_judgeCounts[JudgeResult.Great] * GREAT_WEIGHT) +
                               (_judgeCounts[JudgeResult.Good] * GOOD_WEIGHT);

            return Mathf.Round((weightedSum / _totalNotes) * 100000f);
        }

        public int GetCount(JudgeResult result) => _judgeCounts.TryGetValue(result, out int count) ? count : 0;
    }
}
