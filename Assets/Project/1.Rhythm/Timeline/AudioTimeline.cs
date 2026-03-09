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

        public void Initialize(AudioSource source, StageData data)
        {
            _audioSource = source;

            if (_audioSource == null)
            {
                Debug.LogError("[AudioTimeline] AudioSource가 할당되지 않았습니다.");
                return;
            }

            _audioSource.clip = data.masterTrack;
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;

            _playStartTimeOffset = data.playStartTime;
            _isStarted = false;
        }

        public void StartTimeline()
        {
            if (_audioSource == null || _audioSource.clip == null) return;

            _audioSource.Play();
            _isStarted = true;
            Debug.Log("<color=cyan>[AudioTimeline] 음악 재생 시작</color>");
        }

        /// <summary>
        /// 실제 리듬 액션의 기준이 되는 StageTime을 반환
        /// </summary>
        public float GetStageTime()
        {
            if (!_isStarted || _audioSource == null) return -1f;

            float musicTime = _audioSource.time;
            return musicTime - _playStartTimeOffset;
        }

        public void Stop()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
            _isStarted = false;
        }

        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
    }
}