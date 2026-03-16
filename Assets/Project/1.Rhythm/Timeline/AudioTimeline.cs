using Project.Rhythm.Data;
using UnityEngine;

namespace Project.Rhythm.Timeline
{
    /// <summary>
    /// AudioSource를 기반으로 실제 리듬 게임 플레이 시간(StageTime)을 계산하는 타임라인
    /// </summary>
    public class AudioTimeline
    {
        private AudioSource _audioSource;
        private float _playStartTimeOffset;
        private bool _isStarted;

        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="source"></param>
        /// <param name="data"></param>
        public void Initialize(AudioSource source, StageData data)
        {
            _audioSource = source;

            _audioSource.clip = data.masterTrack;
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;
            _audioSource.Stop();

            _playStartTimeOffset = data.playStartTime;
            _isStarted = false;
        }

        public void StartTimeline()
        {
            if (_audioSource == null || _audioSource.clip == null)
                return;

            _audioSource.Play();
            _isStarted = true;
        }

        public float GetStageTime()
        {
            if (!_isStarted)
                return -1f;

            return _audioSource.time - _playStartTimeOffset;
        }

        public void Stop()
        {
            if (_audioSource != null)
                _audioSource.Stop();

            _isStarted = false;
        }
    }
}