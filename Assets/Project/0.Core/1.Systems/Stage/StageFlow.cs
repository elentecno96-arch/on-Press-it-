using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using Project.Rhythm.Data;
using Project.Rhythm.Judgement;
using Project.Rhythm.Presentation;
using Project.Rhythm.Timeline;
using System;
using System.Threading;

namespace Project.Core.Systems.Stage
{
    /// <summary>
    /// 곡의 지연 시작, 완주 대기, 점수 기록, 업적 체크 등 "시간의 흐름"에 따른 비동기 시퀀스를 담당
    /// 스테이지 매니저에서 분리됨
    /// </summary>
    public class StageFlow
    {
        private readonly JudgementSystem _judgement;
        private readonly AudioTimeline _timeline;
        private readonly StagePresenter _presenter;

        public StageFlow(JudgementSystem judgement, AudioTimeline timeline, StagePresenter presenter)
        {
            _judgement = judgement;
            _timeline = timeline;
            _presenter = presenter;
        }

        public async UniTask PlaySequence(StageData data, float delay, CancellationToken token)
        {
            try
            {
                await UniTask.Yield();
                await UniTask.Delay((int)(delay * 1000), cancellationToken: token);

                _timeline.StartTimeline();

                await UniTask.WaitUntil(() => _timeline.GetStageTime() >= data.endPosition, cancellationToken: token);

                float score = _judgement.CalculateFinalScore();
                _judgement.FinalizeAndSaveResult();

                ProcessAchievements(data, score);
                _presenter.ShowResult(
                    _judgement.GetCount(JudgeResult.Perfect),
                    _judgement.GetCount(JudgeResult.Great),
                    _judgement.GetCount(JudgeResult.Good),
                    _judgement.GetCount(JudgeResult.Miss)
                );
            }
            catch (OperationCanceledException) { }
        }

        private void ProcessAchievements(StageData data, float score)
        {
            if (AchievementManager.Instance == null) return;

            int pCount = _judgement.GetCount(JudgeResult.Perfect);

            AchievementManager.Instance.CheckStageAchievements(data, pCount, score, true);
        }
    }
}


