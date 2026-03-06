using Cysharp.Threading.Tasks;
using Project.Rhythm;
using Project.Rhythm.Data;
using Project.Rhythm.Timeline;
using System;
using UnityEngine;

namespace Project.Core.Managers
{
    /// <summary>
    /// 게임 씬에서 스테이지를 관리하는 매니저 (싱글톤 아님)
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        private AudioTimeline _audioTimeline;
        // private RhythmEventSystem _eventSystem;
        // private JudgementSystem _judgementSystem;

        public event Action OnStageInitialized;
        private bool _isInitialized;

        public async UniTask Initialize()
        {
            if (_isInitialized) return;

            // 1. 스테이지 데이터 가져오기 (GameManager 등 전역 상태 참조)
            // var stageData = GameManager.Instance.CurrentStageData;

            // 2. 하위 시스템 순차적 초기화
            _audioTimeline = new AudioTimeline();
            //_audioTimeline.Initialize(musicSource,stageData);
            // _audioTimeline.SetTimeRegion(stageData.startTime, stageData.endTime);

            // 3. 이벤트 시스템 및 판정 시스템 초기화 대기
            // await _eventSystem.Initialize(stageData);
            // await _judgementSystem.Initialize();

            await UniTask.Yield(); // 필요 시 한 프레임 대기

            _isInitialized = true;

            // 로딩 매니저에게 "준비 완료" 신호 발송
            OnStageInitialized?.Invoke();
            Debug.Log("[StageManager] 씬 내부 모든 시스템 초기화 완료");
        }
    }
}