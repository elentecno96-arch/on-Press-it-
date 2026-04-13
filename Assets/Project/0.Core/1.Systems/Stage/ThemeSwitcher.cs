using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using Project.Core.Ui.GlobalUi;
using Project.Rhythm.Data;
using Project.Rhythm.Event;
using Project.Rhythm.Judgement;
using Project.Rhythm.Presentation;
using Project.Rhythm.Timeline;
using System;

namespace Project.Core.Systems.Stage
{
    /// <summary>
    /// 테마 스위칭 시스템
    /// 스테이지 매니저에서 분리됨
    /// </summary>
    public class ThemeSwitcher
    {
        private readonly JudgementSystem _judgement;
        private readonly RhythmEventSystem _eventSystem;
        private readonly CountdownSystem _countdownSystem;
        private readonly AudioTimeline _timeline;
        private readonly StagePresenter _presenter;

        public ThemeSwitcher(
            JudgementSystem judgement,
            RhythmEventSystem eventSystem,
            CountdownSystem countdownSystem,
            AudioTimeline timeline,
            StagePresenter presenter)
        {
            _judgement = judgement;
            _eventSystem = eventSystem;
            _countdownSystem = countdownSystem;
            _timeline = timeline;
            _presenter = presenter;
        }

        public async UniTask Switch(StageThemeType theme, Action onClearNotes)
        {
            InputManager.Instance.SetBlockInput(true);
            await GlobalUIPresenter.Instance.FadeIn(0.1f);

            _judgement.ForceCompleteAll();
            onClearNotes?.Invoke(); 

            _presenter.ChangeTheme(theme);

            float syncTime = _timeline.GetStageTime();
            _eventSystem.SyncToTime(syncTime);
            _countdownSystem.SyncToTime(syncTime);

            await GlobalUIPresenter.Instance.FadeOut(0.1f);
            InputManager.Instance.SetBlockInput(false);
        }
    }
}
