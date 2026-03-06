using Project.Rhythm.Data;
using UnityEngine;

namespace Project.Rhythm.Timeline
{
    /// <summary>
    /// 게임의 리듬 기준이 되는 마스터 타임
    /// </summary>
    public class AudioTimeline
    {
        private AudioSource audioSource;
        private float _stageStartTime; //실제 게임 위치
        private bool isPlaying;

        public void Initialize(AudioSource source, StageData data)
        {
            audioSource = source;
            audioSource.clip = data.masterTrack;

            audioSource.time = data.startPosition;
            _stageStartTime = data.playStartTime;
        }

        public void StartTimeline()
        {
            audioSource.Play();
            isPlaying = true;
        }

        public float GetTime()
        {
            if (!isPlaying) return 0f;
            return audioSource.time - _stageStartTime;
        }
    }
}