using Project.Core.Managers;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
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

        private float _perfectWin, _greatWin, _goodWin, _missWin;

        private readonly Queue<JudgeData> _judgeQueue = new();
        private readonly Dictionary<JudgeResult, int> _judgeCounts = new();
        private float _secondsPerBeat;
        private JudgeData? _activeHoldNote = null;
        private int _currentStageIndex;

        public event Action<JudgeResult, Note.Note> OnJudged;

        private int _totalNotes; // 총 노트 수 (점수 계산용)

        public bool IsHolding => _activeHoldNote.HasValue;

        public void Initialize(StageData data)
        {
            _currentStageIndex = data.stageIndex;
            _secondsPerBeat = 60f / data.bpm;
            _judgeQueue.Clear();
            _activeHoldNote = null;
            _judgeCounts.Clear();

            foreach (JudgeResult res in Enum.GetValues(typeof(JudgeResult)))
                _judgeCounts[res] = 0;

            _totalNotes = 0;
            if (data.actions != null)
            {
                foreach (var action in data.actions)
                {
                    if (action.noteType != NoteType.Signal)
                        _totalNotes++;
                }
            }

            _perfectWin = data.perfectWindow;
            _greatWin = data.greatWindow;
            _goodWin = data.goodWindow;
            _missWin = data.missWindow;
        }
        /// <summary>
        /// 게임 종료 시 호출: 점수 저장 및 간단한 결과 콘솔 출력
        /// </summary>
        public void FinalizeAndSaveResult()
        {
            // 만점 고정 점수 계산 (최대 100,000점)
            float score = CalculateFinalScore();

            // 데이터 저장
            PlayerManager.Instance.SaveBestScore(_currentStageIndex, score);

            // 결과 출력
            Debug.Log($"<color=yellow><b>[STAGE {_currentStageIndex} CLEAR]</b></color> " +
                      $"Final Score: {score:N0} / 100,000 | " +
                      $"P:{_judgeCounts[JudgeResult.Perfect]} G:{_judgeCounts[JudgeResult.Great]} " +
                      $"Go:{_judgeCounts[JudgeResult.Good]} M:{_judgeCounts[JudgeResult.Miss]}");
        }

        /// <summary>
        /// 만점 고정 점수 계산 로직
        /// </summary>
        public float CalculateFinalScore()
        {
            if (_totalNotes <= 0) return 0f;

            // 가중치 합산
            float weightedSum = (_judgeCounts[JudgeResult.Perfect] * 1.0f) +
                               (_judgeCounts[JudgeResult.Great] * 0.7f) +
                               (_judgeCounts[JudgeResult.Good] * 0.4f);

            // (가중치 합 / 총 노트 수) * 100,000
            float finalScore = (weightedSum / _totalNotes) * 100000f;

            // 부동 소수점 오차 방지를 위해 반올림 처리
            return Mathf.Round(finalScore);
        }


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


        public void ProcessTap(float stageTime)
        {
            if (_judgeQueue.Count == 0) return;
            var target = _judgeQueue.Peek();
            if (target.action.type == PatternType.Hold || target.action.type == PatternType.Slide) return;
            float absDiff = Mathf.Abs(stageTime - target.targetTime);
            if (absDiff <= _missWin) ApplyResult(target, CalculateResult(absDiff));
        }

        public Project.Rhythm.Note.Note GetCurrentHoldNote()
        {
            return _activeHoldNote?.note;
        }

        public void ProcessSlide(float stageTime)
        {
            if (_judgeQueue.Count == 0) return;
            var target = _judgeQueue.Peek();
            if (target.action.type == PatternType.Slide)
            {
                float absDiff = Mathf.Abs(stageTime - target.targetTime);
                if (absDiff <= _missWin) ApplyResult(target, CalculateResult(absDiff));
            }
        }

        public void ProcessHoldDown(float stageTime)
        {
            if (_judgeQueue.Count == 0 || _activeHoldNote.HasValue) return;

            var target = _judgeQueue.Peek();
            if (target.action.type == PatternType.Hold)
            {
                float absDiff = Mathf.Abs(stageTime - target.targetTime);

                if (absDiff <= _missWin)
                {
                    _judgeQueue.Dequeue();
                    _activeHoldNote = target;
                }
            }
        }

        public void ProcessHoldUp(float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;

            var target = _activeHoldNote.Value;
            float releaseTime = target.targetTime + (target.action.duration * _secondsPerBeat);

            float absDiff = Mathf.Abs(stageTime - releaseTime);
            JudgeResult result = CalculateResult(absDiff);

            LogAndNotify(result, target.note);
            _activeHoldNote = null;
        }

        public void UpdateHoldCheck(bool isPressing, float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;

            var target = _activeHoldNote.Value;
            float releaseTime = target.targetTime + (target.action.duration * _secondsPerBeat);

            if (isPressing)
            {
                if (stageTime > releaseTime + _missWin)
                {
                    LogAndNotify(JudgeResult.Miss, target.note); 
                    _activeHoldNote = null; 
                    return;
                }
            }
            else
            {
                float absDiff = Mathf.Abs(stageTime - releaseTime);
                if (stageTime < releaseTime - _missWin)
                {
                    LogAndNotify(JudgeResult.Miss, target.note);
                }
                else
                {
                    LogAndNotify(CalculateResult(absDiff), target.note);
                }

                _activeHoldNote = null;
            }
        }

        private void ApplyResult(JudgeData target, JudgeResult result) { _judgeQueue.Dequeue(); LogAndNotify(result, target.note); }
        private void LogAndNotify(JudgeResult result, Note.Note note)
        {
            _judgeCounts[result]++;
            OnJudged?.Invoke(result, note);
        }

        private JudgeResult CalculateResult(float absDiff)
        {
            if (absDiff <= _perfectWin) return JudgeResult.Perfect;
            if (absDiff <= _greatWin) return JudgeResult.Great;
            if (absDiff <= _goodWin) return JudgeResult.Good;
            return JudgeResult.Miss;
        }
        public void CheckMiss(float stageTime)
        {
            while (_judgeQueue.Count > 0 && stageTime > _judgeQueue.Peek().targetTime + _missWin)
                ApplyResult(_judgeQueue.Peek(), JudgeResult.Miss);
        }

        public float GetHoldProgress(float stageTime)
        {
            if (!_activeHoldNote.HasValue) return 0f;
            return Mathf.Clamp01((stageTime - _activeHoldNote.Value.targetTime) / (_activeHoldNote.Value.action.duration * _secondsPerBeat));
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

        public int GetCount(JudgeResult result) => _judgeCounts[result];
    }
}