using Project.Rhythm.Data;
using System.Collections.Generic;
using Project.Rhythm.Data.Struct;
using UnityEngine;

[System.Serializable]
public class StageTimelineSegment
{
    public string segmentName;   // 구분용 이름 (예: "Intro", "Phase 1")
    public float startBeat;      // 시작 박자
    public float endBeat;        // 종료 박자
    public int stageIndex;       // 사용할 스테이지 번호 (0: Stage1, 1: Stage2, 2: Stage3)
    public bool useFade = true;  // 전환 시 페이드 사용 여부
}

[CreateAssetMenu(fileName = "BossStageData", menuName = "Project/Rhythm/BossStageData")]
public class BossStageData : ScriptableObject
{
    public AudioClip bossMusic;
    public float bpm;

    public List<StageData> stageResources = new List<StageData>(3);

    public List<StageTimelineSegment> timeline = new List<StageTimelineSegment>();

    public List<RhythmAction> bossActions;
}
