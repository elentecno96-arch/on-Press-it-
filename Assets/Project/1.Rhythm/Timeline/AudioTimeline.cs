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
        private float _playStartTimeOffset;                                         //곡의 실제 시작 전 공백 
        private bool _isStarted;

        /// <summary>
        /// 현재 오디오가 실제로 재생 중인지 여부를 반환
        /// </summary>
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

        /// <summary>
        /// 오디오 재생을 시작하고 타임라인 추적을 활성화
        /// </summary>
        public void StartTimeline()
        {
            if (_audioSource == null || _audioSource.clip == null)
                return;

            _audioSource.Play();
            _isStarted = true;
        }

        /// <summary>
        /// 오디오 소스의 현재 재생 시간에서 오프셋을 제외한 '게임 논리 시간'을 계산
        /// </summary>
        /// <returns>게임 진행 시간(초), 시작 전이라면 -1f</returns>
        public float GetStageTime()
        {
            if (!_isStarted)
                return -1f;

            // 여기서 오프셋을 빼줌으로써 첫 번째 노트가 0초에 오도록 교정합니다.
            return _audioSource.time - _playStartTimeOffset;
        }

        /// <summary>
        /// 정지
        /// </summary>
        public void Stop()
        {
            if (_audioSource != null)
                _audioSource.Stop();

            _isStarted = false;
        }
    }
}