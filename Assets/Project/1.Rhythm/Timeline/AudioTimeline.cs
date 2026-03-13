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
        private bool _started;

        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

        public void Initialize(AudioSource source, StageData data)
        {
            _audioSource = source;

            _audioSource.clip = data.masterTrack;
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;

            _playStartTimeOffset = data.playStartTime;
            _started = false;
        }

        public void StartTimeline()
        {
            if (_audioSource == null || _audioSource.clip == null)
                return;

            _audioSource.Play();
            _started = true;
        }

        public float GetStageTime()
        {
            if (!_started)
                return -1f;

            float time = _audioSource.time - _playStartTimeOffset;
            return Mathf.Max(0f, time);
        }

        public void Stop()
        {
            if (_audioSource != null)
                _audioSource.Stop();

            _started = false;
        }
    }
}