using Cysharp.Threading.Tasks;
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

        private AudioTimeline audioTimeline;

        public event Action OnStageInitialized;

        private bool isInitialized;

        public async UniTask Initialize()
        {
            if (isInitialized)
                return;

            await UniTask.Yield();

            // AudioTimeline 생성
            audioTimeline = new AudioTimeline();
            audioTimeline.Initialize(musicSource);

            // EventSystem 생성 예정
            // JudgementSystem 생성 예정

            isInitialized = true;

            Debug.Log("[StageManager] Initialize 완료");

            OnStageInitialized?.Invoke();
        }

        public void StartStage()
        {
            if (!isInitialized)
                return;

            audioTimeline.StartTimeline();
        }

        private void Update()
        {
            if (!isInitialized)
                return;

            float currentTime = audioTimeline.GetTime();

            // EventSystem.Process(currentTime);
            // JudgementSystem.Update(currentTime);
        }
    }
}