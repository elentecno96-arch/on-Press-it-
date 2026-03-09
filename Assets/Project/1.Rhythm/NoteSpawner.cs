using Project.Rhythm.Data;
using Project.Rhythm.Note.Interface;
using Project.Rhythm.Presentation;
using Project.Rhythm.Strategies;
using UnityEngine;

namespace Project.Rhythm
{
    /// <summary>
    /// 노트 생성 담당 클래스
    /// </summary>
    public class NoteSpawner
    {
        private readonly StagePresenter _presenter;
        public NoteSpawner(StagePresenter presenter)
        {
            _presenter = presenter;
        }

        public Project.Rhythm.Note.Note Spawn(RhythmAction action, float targetHitTime, float appearDuration)
        {
            GameObject obj = _presenter.SpawnNote();
            Project.Rhythm.Note.Note note = obj.GetComponent<Project.Rhythm.Note.Note>();

            ISpawnStrategy s = GetSpawnStrategy(action.spawnStrategyId);
            IMoveStrategy m = GetMoveStrategy(action.moveStrategyId);
            IActionStrategy a = GetActionStrategy(action.actionStrategyId);

            note.Setup(s, m, a, targetHitTime, appearDuration);

            return note;
        }

        private ISpawnStrategy GetSpawnStrategy(string id) => new CenterSpawnStrategy();
        private IMoveStrategy GetMoveStrategy(string id) => new ScaleMoveStrategy();
        private IActionStrategy GetActionStrategy(string id) => new TapStrategy();
    }
}