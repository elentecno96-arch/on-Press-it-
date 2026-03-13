using Project.Rhythm.Data.Struct;

public interface INote
{
    float TargetBeat { get; }

    void Initialize(RhythmAction action, float appearDuration);

    void UpdateTime(float currentTime);

    void OnHit();

    void OnMiss();

    bool IsActive { get; }
}