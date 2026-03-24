using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using UnityEngine;
using DG.Tweening;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 스테이지 2 박쥐 노트 
    /// </summary>
    public class BatVisual : BaseRhythmVisual
    {
        [SerializeField] private float fallSpeed = 2500f;
        private const float BASE_SCALE = 1.5f;

        private const float SIGNAL_HOLD_END = 0.6f; // 0.6까지 고정
        private const float BAT_TRANSFORM_START = 0.7f; // 0.7에 변신

        private Vector2 _fallDirection = new Vector2(-0.4f, -1.2f).normalized;
        private bool _isFalling;
        private bool _isBatSpawned;

        protected override void Awake()
        {
            base.Awake();
            RandomSignalSprite();

            transform.localScale = new Vector3(BASE_SCALE, BASE_SCALE, 1f);
        }

        private void RandomSignalSprite()
        {
            if (idleFrames != null && idleFrames.Length > 0)
            {
                targetImage.sprite = idleFrames[Random.Range(0, idleFrames.Length)];
            }
        }

        protected override void Update()
        {
            if (_isFalling)
            {
                targetImage.rectTransform.anchoredPosition += _fallDirection * fallSpeed * Time.deltaTime;
                targetImage.rectTransform.Rotate(Vector3.forward * 600f * Time.deltaTime);
                return;
            }

            if (!_isBatSpawned)
            {
                base.Update();
            }
        }

        public override void UpdateVisual(float progress)
        {
            if (_isFalling || _isJudged) return;

            float targetScale = 1.0f; 

            if (progress <= SIGNAL_HOLD_END)
            {
                targetScale = 0.5f; 
            }

            else if (progress <= BAT_TRANSFORM_START)
            {
                targetScale = 0.5f;
            }

            else
            {
                if (!_isBatSpawned)
                {
                    _isBatSpawned = true;
                    _isLooping = false;
                    if (actionFrames != null && actionFrames.Length > 0)
                        targetImage.sprite = actionFrames[Random.Range(0, actionFrames.Length)];
                }
                float t = (progress - BAT_TRANSFORM_START) / (1.0f - BAT_TRANSFORM_START);
                float acceleratedT = t * t * t;
                targetScale = Mathf.Lerp(0.5f, 1.0f, acceleratedT);
            }

            if (progress > 1.0f)
            {
                float t = (progress - 1.0f);
                targetScale = Mathf.Lerp(1.0f, 3.0f, t);
            }

            float finalScale = targetScale * BASE_SCALE;
            transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        public override void PlayAction(PatternType type)
        {
            if (type == PatternType.Signal)
            {
                PlaySfx(actionSfx);
                targetImage.color = Color.red;
                transform.DOScale(Vector3.one * (BASE_SCALE * 0.6f), 0.1f).OnComplete(() => {
                    targetImage.color = Color.white;
                    transform.DOScale(Vector3.one * (BASE_SCALE * 0.5f), 0.1f);
                });
            }
        }

        public override void PlayAction(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                if (successFrames.Length > 0)
                    targetImage.sprite = successFrames[Random.Range(0, successFrames.Length)];

                _isFalling = true; 
            }
            else
            {
                PlaySfx(missSfx);
                if (missFrames.Length > 0)
                    targetImage.sprite = missFrames[Random.Range(0, missFrames.Length)];

                targetImage.color = Color.red;
                targetImage.DOFade(0, 0.2f).OnComplete(() => {
                });
            }
        }

        public override void ResetVisual()
        {
            base.ResetVisual();
            _isFalling = false;
            _isBatSpawned = false;
            targetImage.rectTransform.localRotation = Quaternion.identity;
            targetImage.color = Color.white;
            transform.localScale = new Vector3(BASE_SCALE, BASE_SCALE, 1f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            transform.DOKill();
            targetImage.DOKill();

            _isFalling = false;
            _isBatSpawned = false;
            _isJudged = false;

            targetImage.rectTransform.anchoredPosition = Vector2.zero;
            targetImage.rectTransform.localRotation = Quaternion.identity;
            targetImage.color = Color.white;
            transform.localScale = new Vector3(BASE_SCALE, BASE_SCALE, 1f);
        }
    }
}