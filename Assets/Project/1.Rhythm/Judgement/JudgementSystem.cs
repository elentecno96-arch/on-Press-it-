using Project.Core.Managers;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Data.Struct;
using Project.Rhythm.Note;
using Rhythm.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Judgement
{
    public enum JudgeResult { Perfect, Great, Good, Miss }

    /// <summary>
    /// 판정을 담당하는 시스템
    /// 노트가 등록되면 타이밍을 계산하여 판정 결과를 결정하고, ScoreSystem에 결과를 전달
    /// </summary>
    public class JudgementSystem
    {
        private struct JudgeData
        {
            public RhythmAction action;
            public Note.Note note;
            public float targetTime;
        }

        private readonly ICurrentTime _timeProvider;
        private readonly Queue<JudgeData> _judgeQueue = new();
        private readonly ScoreSystem _scoreSystem = new ScoreSystem();

        private float _perfectWin, _greatWin, _goodWin, _missWin;
        private float _secondsPerBeat;
        private JudgeData? _activeHoldNote = null;
        private int _currentStageIndex;
        
        public event Action<JudgeResult, Note.Note> OnJudged;
        public bool IsHolding => _activeHoldNote.HasValue;

        public JudgementSystem(ICurrentTime timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public void Initialize(StageData data)
        {
            _currentStageIndex = data.stageIndex;
            _secondsPerBeat = 60f / data.bpm;
            _judgeQueue.Clear();
            _activeHoldNote = null;

            int total = 0;
            if (data.actions != null)
            {
                for (int i = 0; i < data.actions.Count; i++)
                {
                    if (data.actions[i].noteType != NoteType.Signal) total++;
                }
            }
            _scoreSystem.Initialize(total);

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
            PlayerManager.Instance.SaveStageResult(_currentStageIndex, score);

            Debug.Log($"<color=yellow><b>[STAGE {_currentStageIndex} CLEAR]</b></color> " +
                      $"Score: {score:N0} | P:{GetCount(JudgeResult.Perfect)} G:{GetCount(JudgeResult.Great)} " +
                      $"Go:{GetCount(JudgeResult.Good)} M:{GetCount(JudgeResult.Miss)}");
        }

        /// <summary>
        /// 만점 고정 점수 계산 로직
        /// </summary>
        public float CalculateFinalScore() => _scoreSystem.CalculateScore();
        public int GetCount(JudgeResult result) => _scoreSystem.GetCount(result);

        public void RegisterNote(RhythmAction action, Note.Note note)
        {
            if (note == null || note.Judged) return;
            _judgeQueue.Enqueue(new JudgeData
            {
                action = action,
                note = note,
                targetTime = action.beat * _secondsPerBeat
            });
        }


        public void ProcessTap()
        {
            if (_judgeQueue.Count == 0) return;
            var target = _judgeQueue.Peek();
            if (target.action.type == PatternType.Hold || target.action.type == PatternType.Slide) return;

            float absDiff = Mathf.Abs(_timeProvider.CurrentTime - target.targetTime);
            if (absDiff <= _missWin) ApplyResult(target, CalculateResult(absDiff));
        }

        public Project.Rhythm.Note.Note GetCurrentHoldNote()
        {
            return _activeHoldNote?.note;
        }

        public void ProcessSlide()
        {
            if (_judgeQueue.Count == 0) return;
            var target = _judgeQueue.Peek();
            if (target.action.type != PatternType.Slide) return;

            float absDiff = Mathf.Abs(_timeProvider.CurrentTime - target.targetTime);
            if (absDiff <= _missWin) ApplyResult(target, CalculateResult(absDiff));
        }

        public void ProcessHoldDown()
        {
            if (_judgeQueue.Count == 0 || _activeHoldNote.HasValue) return;
            var target = _judgeQueue.Peek();
            if (target.action.type != PatternType.Hold) return;

            float absDiff = Mathf.Abs(_timeProvider.CurrentTime - target.targetTime);
            if (absDiff <= _missWin)
            {
                _judgeQueue.Dequeue();
                _activeHoldNote = target;
            }
        }

        public void UpdateHoldCheck(bool isPressing)
        {
            if (!_activeHoldNote.HasValue) return;

            float stageTime = _timeProvider.CurrentTime;
            var target = _activeHoldNote.Value;

            float releaseTime = target.targetTime + (target.action.duration * _secondsPerBeat);

            if (isPressing)
            {
                if (stageTime > releaseTime + _missWin)
                {
                    _activeHoldNote = null;
                    LogAndNotify(JudgeResult.Miss, target.note);
                }
            }
            else
            {
                float absDiff = Mathf.Abs(stageTime - releaseTime);
                _activeHoldNote = null;

                if (stageTime < releaseTime - _missWin)
                {
                    LogAndNotify(JudgeResult.Miss, target.note);
                }
                else
                {
                    LogAndNotify(CalculateResult(absDiff), target.note);
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
            _scoreSystem.AddResult(result);
            OnJudged?.Invoke(result, note);
        }

        private JudgeResult CalculateResult(float absDiff)
        {
            if (absDiff <= _perfectWin) return JudgeResult.Perfect;
            if (absDiff <= _greatWin) return JudgeResult.Great;
            if (absDiff <= _goodWin) return JudgeResult.Good;
            return JudgeResult.Miss;
        }

        public void CheckMiss()
        {
            float stageTime = _timeProvider.CurrentTime;

            while (_judgeQueue.Count > 0 && stageTime > _judgeQueue.Peek().targetTime + _missWin)
                ApplyResult(_judgeQueue.Peek(), JudgeResult.Miss);
        }

        public float GetHoldProgress()
        {
            if (!_activeHoldNote.HasValue) return 0f;
            float duration = _activeHoldNote.Value.action.duration * _secondsPerBeat;
            if (duration <= 0) return 1f;
            return Mathf.Clamp01((_timeProvider.CurrentTime - _activeHoldNote.Value.targetTime) / duration);
        }

        public void Reset() { _judgeQueue.Clear(); _activeHoldNote = null; _scoreSystem.Initialize(0); }

        public void SyncToTime()
        {
            float stageTime = _timeProvider.CurrentTime;
            while (_judgeQueue.Count > 0 && stageTime > _judgeQueue.Peek().targetTime + _missWin)
                _judgeQueue.Dequeue();

            if (_activeHoldNote.HasValue)
            {
                var hold = _activeHoldNote.Value;
                if (stageTime > (hold.targetTime + hold.action.duration * _secondsPerBeat) + _missWin)
                    _activeHoldNote = null;
            }
        }

        public void ForceCompleteAll()
        {
            while (_judgeQueue.Count > 0) ApplyResult(_judgeQueue.Peek(), JudgeResult.Miss);
            if (_activeHoldNote.HasValue)
            {
                LogAndNotify(JudgeResult.Miss, _activeHoldNote.Value.note);
                _activeHoldNote = null;
            }
        }
    }
}