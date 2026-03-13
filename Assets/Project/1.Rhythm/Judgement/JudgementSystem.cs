using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Judgement
{
    public enum JudgeResult { Perfect, Great, Good, Miss }

    public class JudgementSystem
    {
        private struct JudgeData
        {
            public RhythmAction action;
            public Note.Note note;
            public float targetTime;
        }

        //StageData로 위치 변경 예정
        private const float PERFECT_WINDOW = 0.12f;
        private const float GREAT_WINDOW = 0.21f;
        private const float GOOD_WINDOW = 0.27f;
        private const float MISS_WINDOW = 0.34f;

        private readonly Queue<JudgeData> _judgeQueue = new();
       // private StageData _stageData;
        private float _secondsPerBeat;
        private JudgeData? _activeHoldNote = null;

        public event Action<JudgeResult, Note.Note> OnJudged;


        public void Initialize(StageData data)
        {
            _secondsPerBeat = 60f / data.bpm;
            _judgeQueue.Clear();
            _activeHoldNote = null;
        }

        public void RegisterNote(RhythmAction action, Note.Note note)
        {
            float targetTime = action.beat * _secondsPerBeat;

            _judgeQueue.Enqueue(new JudgeData
            {
                action = action,
                note = note,
                targetTime = targetTime
            });
        }

        public void ProcessInput(PatternType inputType, float stageTime)
        {
            if (_judgeQueue.Count == 0) return;

            var target = _judgeQueue.Peek();
            float absDiff = Mathf.Abs(stageTime - target.targetTime);

            if (absDiff > MISS_WINDOW) return;

            JudgeResult result = CalculateResult(absDiff);
            ApplyResult(target, result);
        }


        public void ProcessInputDown(float stageTime)
        {
            if (_judgeQueue.Count == 0 || _activeHoldNote.HasValue) return;

            var target = _judgeQueue.Peek();

            float absDiff = Mathf.Abs(stageTime - target.targetTime);

            if (absDiff <= MISS_WINDOW)
            {
                _judgeQueue.Dequeue();
                _activeHoldNote = target;
                OnJudged?.Invoke(JudgeResult.Perfect, target.note);
            }
        }


        public void ProcessInputUp(float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;

            var target = _activeHoldNote.Value;
            float targetReleaseTime = target.targetTime + (target.action.duration * _secondsPerBeat);
            float absDiff = Mathf.Abs(stageTime - targetReleaseTime);

            JudgeResult result = CalculateResult(absDiff);

            PrintJudgeLog(result);
            OnJudged?.Invoke(result, target.note);
            _activeHoldNote = null;
        }

        public void UpdateHoldCheck(bool isPressing, float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;

            if (!isPressing)
            {
                float targetReleaseTime = _activeHoldNote.Value.targetTime + (_activeHoldNote.Value.action.duration * _secondsPerBeat);

                if (stageTime < targetReleaseTime - GOOD_WINDOW)
                {
                    PrintJudgeLog(JudgeResult.Miss);
                    OnJudged?.Invoke(JudgeResult.Miss, _activeHoldNote.Value.note);
                    _activeHoldNote = null;
                }
            }
        }

        private void ApplyResult(JudgeData target, JudgeResult result)
        {
            _judgeQueue.Dequeue();
            PrintJudgeLog(result);
            OnJudged?.Invoke(result, target.note);
        }

        private JudgeResult CalculateResult(float absDiff)
        {
            if (absDiff <= PERFECT_WINDOW) return JudgeResult.Perfect;
            if (absDiff <= GREAT_WINDOW) return JudgeResult.Great;
            if (absDiff <= GOOD_WINDOW) return JudgeResult.Good;
            return JudgeResult.Miss;
        }

        public void CheckMiss(float stageTime)
        {
            while (_judgeQueue.Count > 0)
            {
                var target = _judgeQueue.Peek();

                if (stageTime > target.targetTime + MISS_WINDOW)
                {
                    _judgeQueue.Dequeue();
                    PrintJudgeLog(JudgeResult.Miss);
                    OnJudged?.Invoke(JudgeResult.Miss, target.note);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 판정 로그 확인용
        /// </summary>
        /// <param name="result"></param>
        private void PrintJudgeLog(JudgeResult result)
        {
            string color = result switch
            {
                JudgeResult.Perfect => "cyan",
                JudgeResult.Great => "green",
                JudgeResult.Good => "yellow",
                JudgeResult.Miss => "red",
                _ => "white"
            };

            Debug.Log($"<color={color}>[Judgement]</color> <b>{result.ToString().ToUpper()}</b>");
        }
    }
}