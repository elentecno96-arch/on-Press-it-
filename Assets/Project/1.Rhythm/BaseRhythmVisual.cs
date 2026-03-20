using Project.Core.Managers;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

        protected Sprite[] _currentAnimation;
        protected int _currentFrameIndex;
        protected float _frameTimer;
        protected float _currentFrameRate; //개별 프레임 속도 처리 캐싱용
        protected bool _isJudged;
        protected bool _isLooping;
        protected bool _isHolding;

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
            if (_currentAnimation == null || _currentAnimation.Length == 0) return;

            if (_currentAnimation.Length > 1)
            {
                _frameTimer += Time.deltaTime;

                if (_frameTimer >= _currentFrameRate)
                {
                    _frameTimer = 0f;

                    if (!_isLooping && _currentFrameIndex >= _currentAnimation.Length - 1)
                    {
                        OnAnimationComplete();
                        return;
                    }

                    _currentFrameIndex++;

                    if (_isLooping)
                    {
                        _currentFrameIndex %= _currentAnimation.Length;
                    }

                    targetImage.sprite = _currentAnimation[_currentFrameIndex];
                }
            }
            else
            {
                targetImage.sprite = _currentAnimation[0];
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
            transform.DOKill();
            SetAnimation(idleFrames, idleFrameRate, true);
        }

        public virtual void UpdateVisual(float progress) { }
        public virtual void StopHoldAction() { }
        public virtual void StartCountdown(float duration) { }
    }
}