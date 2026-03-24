using Project.Core.Managers;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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
            // 1. 점수 계산
            float score = (_judgeCounts[JudgeResult.Perfect] * 100f) +
                          (_judgeCounts[JudgeResult.Great] * 70f) +
                          (_judgeCounts[JudgeResult.Good] * 40f);
            // 2. 데이터 저장
            PlayerManager.Instance.SaveBestScore(_currentStageIndex, score);

            // 3. 콘솔에 한 줄로 요약 출력 (매우 간소화)
            Debug.Log($"<color=yellow><b>[STAGE {_currentStageIndex} CLEAR]</b></color> " +
                      $"Score: {score} | Perfect:{_judgeCounts[JudgeResult.Perfect]} Great:{_judgeCounts[JudgeResult.Great]} " +
                      $"Good:{_judgeCounts[JudgeResult.Good]} Miss:{_judgeCounts[JudgeResult.Miss]}");
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
                if (Mathf.Abs(stageTime - target.targetTime) <= _missWin)
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
            float targetReleaseTime = target.targetTime + (target.action.duration * _secondsPerBeat);
            LogAndNotify(CalculateResult(Mathf.Abs(stageTime - targetReleaseTime)), target.note);
            _activeHoldNote = null;
        }
        public void UpdateHoldCheck(bool isPressing, float stageTime)
        {
            if (!_activeHoldNote.HasValue) return;
            if (!isPressing)
            {
                float releaseTime = _activeHoldNote.Value.targetTime + (_activeHoldNote.Value.action.duration * _secondsPerBeat);
                if (stageTime < releaseTime - _goodWin)
                {
                    LogAndNotify(JudgeResult.Miss, _activeHoldNote.Value.note);
                    _activeHoldNote = null;
                }
            }
        }
        private void ApplyResult(JudgeData target, JudgeResult result) { _judgeQueue.Dequeue(); LogAndNotify(result, target.note); }
        private void LogAndNotify(JudgeResult result, Note.Note note)
        {
            _judgeCounts[result]++;
            // 개별 노트 판정 로그 (필요 없으면 주석 처리 하세요)
            // PrintJudgeLog(result);
            OnJudged?.Invoke(result, note);
        }

        public void ForceCompleteAll()
        {
            if (_currentHoldNote != null)
            {
                _currentHoldNote.ForceComplete();
                _currentHoldNote = null;
            }

            _registeredNotes.Clear();
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
        private void PrintJudgeLog(JudgeResult result) // 개별 판정 로그
        {
            string color = result switch { JudgeResult.Perfect => "cyan", JudgeResult.Great => "green", JudgeResult.Good => "yellow", _ => "red" };
            Debug.Log($"<color={color}>[{result}]</color>");
        }
    }
}