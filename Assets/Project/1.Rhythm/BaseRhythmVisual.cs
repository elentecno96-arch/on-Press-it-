using DG.Tweening;
using Project.Core.Managers;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using Project.Rhythm.Timeline;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Rhythm.Visual
{
    public abstract class BaseRhythmVisual : MonoBehaviour, ITouchVisual
    {
        [SerializeField] protected Image targetImage;

        [SerializeField] protected Sprite[] idleFrames;
        [SerializeField] protected Sprite[] actionFrames;
        [SerializeField] protected Sprite[] successFrames;
        [SerializeField] protected Sprite[] missFrames;

        [SerializeField] protected float idleFrameRate = 0.1f;
        [SerializeField] protected float actionFrameRate = 0.1f;
        [SerializeField] protected float successFrameRate = 0.1f;
        [SerializeField] protected float missFrameRate = 0.1f;

        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip actionSfx;  // 입력/소환 시
        [SerializeField] protected AudioClip successSfx; // 성공 시
        [SerializeField] protected AudioClip missSfx;    // 실패 시

        protected float _bpm;
        protected AudioTimeline _timeline;

        protected float CurrentTime => _timeline?.GetStageTime() ?? 0f;
        protected float CurrentBeat => (CurrentTime * _bpm) / 60f;

        [SerializeField] protected AudioClip countSfx;
        public virtual void SetBpm(float bpm) => _bpm = bpm;

        protected Sprite[] _currentAnimation;
        protected int _currentFrameIndex;
        protected float _frameTimer;
        protected float _currentFrameRate; //개별 프레임 속도 처리 캐싱용
        protected bool _isJudged;
        protected bool _isLooping;
        protected bool _isHolding;

        protected CancellationTokenSource _visualCts;

        public virtual void SetTimeProvider(AudioTimeline timeline)
        {
            _timeline = timeline;
        }

        protected virtual void Awake()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource != null && AudioManager.Instance != null)
            {
                audioSource.outputAudioMixerGroup = AudioManager.Instance.SFXGroup;
            }

            SetAnimation(idleFrames, idleFrameRate, true);
        }

        protected virtual void Update()
        {
            if (_currentAnimation == null || _currentAnimation.Length <= 1)
            {
                // 프레임이 1개 이하면 스프라이트만 교체하고 연산 중지
                if (_currentAnimation != null && _currentAnimation.Length == 1)
                    targetImage.sprite = _currentAnimation[0];
                return;
            }

            _frameTimer += Time.deltaTime;
            if (_frameTimer >= _currentFrameRate)
            {
                _frameTimer = 0f;
                _currentFrameIndex++;

                if (!_isLooping && _currentFrameIndex >= _currentAnimation.Length)
                {
                    OnAnimationComplete();
                    return;
                }

                if (_isLooping) _currentFrameIndex %= _currentAnimation.Length;

                // 인덱스 안전 범위 체크 후 할당
                if (_currentFrameIndex < _currentAnimation.Length)
                    targetImage.sprite = _currentAnimation[_currentFrameIndex];
            }
        }

        protected void SetAnimation(Sprite[] frames, float rate, bool loop)
        {
            if (frames == null || frames.Length == 0) return;

            _currentAnimation = frames;
            _currentFrameRate = rate; 
            _isLooping = loop;
            _currentFrameIndex = 0;
            _frameTimer = 0f;

            if (targetImage != null)
                targetImage.sprite = _currentAnimation[0];
        }

        protected virtual void OnAnimationComplete()
        {
            _currentFrameIndex = _currentAnimation.Length - 1; // 마지막 프레임 고정
            if (idleFrames != null && idleFrames.Length > 0)
                SetAnimation(idleFrames, idleFrameRate, true);
        }

        protected void PlaySfx(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public abstract void PlayAction(PatternType type);
        public abstract void PlayAction(JudgeResult result);

        public virtual void ResetVisual()
        {
            _isJudged = false;
            _isHolding = false;

            transform.DOKill();

            SetAnimation(idleFrames, idleFrameRate, true);
        }

        protected virtual void OnDisable()
        {
            ClearVisualCts();
        }

        protected void ClearVisualCts()
        {
            if (_visualCts != null)
            {
                _visualCts.Cancel();
                _visualCts.Dispose();
                _visualCts = null;
            }
        }

        protected CancellationToken RefreshToken()
        {
            ClearVisualCts();
            _visualCts = new CancellationTokenSource();
            return _visualCts.Token;
        }

        public virtual void UpdateVisual(float progress) { }
        public virtual void StopHoldAction() { }
        public abstract void StartCountdown(float targetBeat);
    }
}