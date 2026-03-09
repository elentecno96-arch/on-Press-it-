using Project.Rhythm.Data;
using Project.Rhythm.Note.State;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Judgement
{
    public enum JudgeResult { Perfect, Great, Good, Miss }

    /// <summary>
    /// 플레이어의 입력과 노트의 타이밍을 비교하여 판정을 내리는 시스템
    /// </summary>
    public class JudgementSystem
    {
        private struct JudgeData
        {
            public RhythmAction action;
            public Note.Note note; 
            public float targetTime;
        }

        private const float PERFECT_WINDOW = 0.05f;
        private const float GREAT_WINDOW = 0.12f;
        private const float GOOD_WINDOW = 0.18f;
        private const float MISS_WINDOW = 0.25f;

        private readonly Queue<JudgeData> _judgeQueue = new();
        private float _secondsPerBeat;

        public event Action<JudgeResult, PatternType> OnJudged;

        public void Initialize(StageData data)
        {
            _secondsPerBeat = 60f / data.bpm;
            _judgeQueue.Clear();
        }

        public void RegisterNote(RhythmAction action, Project.Rhythm.Note.Note note)
        {
            float targetTime = action.beat * _secondsPerBeat;

            _judgeQueue.Enqueue(new JudgeData
            {
                action = action,
                note = note,
                targetTime = targetTime
            });
        }

        /// <summary>
        /// 플레이어 입력 시 호출되어 가장 앞의 노트와 비교 판정
        /// </summary>
        public void ProcessInput(PatternType inputType, float stageTime)
        {
            if (_judgeQueue.Count == 0) return;

            var target = _judgeQueue.Peek();

            if (target.action.type == PatternType.None)
            {
                _judgeQueue.Dequeue();
                return;
            }

            float diff = stageTime - target.targetTime;
            float absDiff = Mathf.Abs(diff);

            if (diff < -GOOD_WINDOW) return;

            if (target.action.type != inputType)
            {
                ApplyResult(target, JudgeResult.Miss, inputType);
                return;
            }

            JudgeResult result = CalculateResult(absDiff);
            ApplyResult(target, result, inputType);
        }

        private void ApplyResult(JudgeData target, JudgeResult result, PatternType inputType)
        {
            _judgeQueue.Dequeue();

            if (target.note != null)
            {
                if (result != JudgeResult.Miss)
                {
                    target.note.ExecuteAction(inputType);
                }
                target.note.ChangeState(new EndState(target.note, result));
            }

            OnJudged?.Invoke(result, inputType);
        }

        private JudgeResult CalculateResult(float absDiff)
        {
            if (absDiff <= PERFECT_WINDOW) return JudgeResult.Perfect;
            if (absDiff <= GREAT_WINDOW) return JudgeResult.Great;
            if (absDiff <= GOOD_WINDOW) return JudgeResult.Good;
            return JudgeResult.Miss;
        }

        /// <summary>
        /// 입력 없이 판정 범위를 벗어난 노트를 Miss 처리합니다. [cite: 2026-03-09]
        /// </summary>
        public void CheckMiss(float stageTime)
        {
            while (_judgeQueue.Count > 0)
            {
                var target = _judgeQueue.Peek();

                if (target.action.type == PatternType.None)
                {
                    _judgeQueue.Dequeue();
                    continue;
                }

                if (stageTime > target.targetTime + MISS_WINDOW)
                {
                    ApplyResult(target, JudgeResult.Miss, target.action.type);
                }
                else break;
            }
        }
    }
}