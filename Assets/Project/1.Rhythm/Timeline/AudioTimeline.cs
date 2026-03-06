using UnityEngine;

namespace Project.Rhythm.Timeline
{
    /// <summary>
    /// 게임의 리듬 기준이 되는 마스터 타임
    /// </summary>
    public class AudioTimeline
    {
        private AudioSource audioSource;
        private float startTime;
        private bool isPlaying;

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="source"></param>
        public void Initialize(AudioSource source)
        {
            audioSource = source;
        }

        /// <summary>
        /// 타임 라인 시작
        /// </summary>
        public void StartTimeline()
        {
            if (audioSource == null)
                return;

            audioSource.Play();

            startTime = audioSource.time;
            isPlaying = true;
        }

        public float GetTime()
        {
            if (!isPlaying || audioSource == null)
                return 0f;

            return audioSource.time - startTime;
        }

        
        public void Stop()
        {
            if (audioSource == null)
                return;

            audioSource.Stop();
            isPlaying = false;
        }
    }
}