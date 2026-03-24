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

        /// <summary>
        /// 추후 SO로 분리되어 가지고 올 판정 세부 변수
        /// </summary>
        private float _perfectWin, _greatWin, _goodWin, _missWin;

        private readonly Queue<JudgeData> _judgeQueue = new();
        private readonly Dictionary<JudgeResult, int> _judgeCounts = new(); // 결과창을 위한 판정 저장소
        private float _secondsPerBeat;
        private JudgeData? _activeHoldNote = null;

        public event Action<JudgeResult, Note.Note> OnJudged;
        public bool IsHolding => _activeHoldNote.HasValue;

        public void Initialize(StageData data)
        {
            _secondsPerBeat = 60f / data.bpm;
            _judgeQueue.Clear();
            _activeHoldNote = null;

            _judgeCounts.Clear();
            foreach (JudgeResult res in Enum.GetValues(typeof(JudgeResult)))
            {
                _judgeCounts[res] = 0;
            }

            //초기 임시 할당
            _perfectWin = data.perfectWindow;
            _greatWin = data.greatWindow;
            _goodWin = data.goodWindow;
            _missWin = data.missWindow;
        }

        /// <summary>
        /// 판정 대기열에 등록
        /// </summary>
        /// <param name="action"></param>
        /// <param name="note"></param>
        public void RegisterNote(RhythmAction action, Note.Note note)
        {
            if (note == null) return;

            if (note.Judged) return; 

            _judgeQueue.Enqueue(new JudgeData
            {
                action = action,
                note = note,
                targetTime = action.beat * _secondsPerBeat
            });
        }

        /// <summary>
        /// 일반 탭 노트 판정 처리
        /// </summary>
        /// <param name="stageTime"></param>
        public void ProcessTap(float stageTime)
        {
            if (_judgeQueue.Count == 0) return;

            var target = _judgeQueue.Peek();

            if (target.action.type == PatternType.Hold || target.action.type == PatternType.Slide) return;

            float absDiff = Mathf.Abs(stageTime - target.targetTime);
            if (absDiff > _missWin) return;

            ApplyResult(target, CalculateResult(absDiff));
        }

        public void ProcessSlide(float stageTime)
        {
            if (_judgeQueue.Count == 0) return;

            var target = _judgeQueue.Peek();

            if (target.action.type != PatternType.Slide) return;

            float absDiff = Mathf.Abs(stageTime - target.targetTime);

            if (absDiff <= _missWin)
            {
                ApplyResult(target, CalculateResult(absDiff));
            }
        }


        public void ProcessHoldDown(float stageTime)
        {
            if (_judgeQueue.Count == 0 || _activeHoldNote.HasValue) return;

            var target = _judgeQueue.Peek();
            if (target.action.type != PatternType.Hold) return;

            float absDiff = Mathf.Abs(stageTime - target.targetTime);

            if (absDiff <= _missWin)
            {
                _judgeQueue.Dequeue();
                _activeHoldNote = target;
            }
        }

        /// <summary>
        /// 홀드 노트를 뗄 때의 판정 처리
        /// </summary>
        /// <param name="stageTime"></param>
        public void ProcessHoldUp(float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;

            var target = _activeHoldNote.Value;
            float targetReleaseTime = target.targetTime + (target.action.duration * _secondsPerBeat);
            float absDiff = Mathf.Abs(stageTime - targetReleaseTime);

            JudgeResult result = CalculateResult(absDiff);

            LogAndNotify(result, target.note);
            _activeHoldNote = null;
        }

        public void UpdateHoldCheck(bool isPressing, float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;

            if (!isPressing)
            {
                float targetReleaseTime = _activeHoldNote.Value.targetTime + (_activeHoldNote.Value.action.duration * _secondsPerBeat);

                if (stageTime < targetReleaseTime - _goodWin)
                {
                    LogAndNotify(JudgeResult.Miss, _activeHoldNote.Value.note);
                    _activeHoldNote = null;
                }
            }
        }

        private void ApplyResult(JudgeData target, JudgeResult result)
        {
            _judgeQueue.Dequeue();
            LogAndNotify(result, target.note);
        }

        private void LogAndNotify(JudgeResult result, Note.Note note)
        {
            _judgeCounts[result]++;

            PrintJudgeLog(result);
            OnJudged?.Invoke(result, note);
        }

        private JudgeResult CalculateResult(float absDiff)
        {
            if (absDiff <= _perfectWin) return JudgeResult.Perfect;
            if (absDiff <= _greatWin) return JudgeResult.Great;
            if (absDiff <= _goodWin) return JudgeResult.Good;
            return JudgeResult.Miss;
        }

        /// <summary>
        /// 시간에 따른 Miss 판정 강제 처리
        /// </summary>
        /// <param name="stageTime"></param>
        public void CheckMiss(float stageTime)
        {
            while (_judgeQueue.Count > 0)
            {
                var target = _judgeQueue.Peek();
                if (stageTime > target.targetTime + _missWin)
                {
                    ApplyResult(target, JudgeResult.Miss);
                }
                else break;
            }
        }

        public float GetHoldProgress(float stageTime)
        {
            if (!_activeHoldNote.HasValue) return 0f;

            var target = _activeHoldNote.Value;

            float startTime = target.targetTime;
            float durationTime = target.action.duration * _secondsPerBeat;

            float elapsed = stageTime - startTime;
            return Mathf.Clamp01(elapsed / durationTime);
        }

        public void Reset()
        {
            _judgeQueue.Clear();
            _activeHoldNote = null;

            _judgeCounts.Clear();
            foreach (JudgeResult res in Enum.GetValues(typeof(JudgeResult)))
            {
                _judgeCounts[res] = 0;
            }
        }

        public void SyncToTime(float stageTime)
        {
            while (_judgeQueue.Count > 0)
            {
                var target = _judgeQueue.Peek();

                if (stageTime > target.targetTime + _missWin)
                {
                    _judgeQueue.Dequeue();
                }
                else break;
            }

            if (_activeHoldNote.HasValue)
            {
                var hold = _activeHoldNote.Value;
                float endTime = hold.targetTime + (hold.action.duration * _secondsPerBeat);

                if (stageTime > endTime + _missWin)
                {
                    _activeHoldNote = null;
                }
            }
        }

        public void ForceCompleteAll()
        {
            while (_judgeQueue.Count > 0)
            {
                var target = _judgeQueue.Dequeue();
                LogAndNotify(JudgeResult.Miss, target.note);
            }

            if (_activeHoldNote.HasValue)
            {
                LogAndNotify(JudgeResult.Miss, _activeHoldNote.Value.note);
                _activeHoldNote = null;
            }
        }

        public Note.Note GetCurrentHoldNote() => _activeHoldNote?.note;
        public int GetCount(JudgeResult result) => _judgeCounts.TryGetValue(result, out int count) ? count : 0;

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