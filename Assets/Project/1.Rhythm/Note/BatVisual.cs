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

        private Vector2 _fallDirection = new Vector2(-0.4f, -1.2f).normalized;
        private bool _isFalling;
        private bool _isBatSpawned;

        protected override void Awake()
        {
            base.Awake();

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

            float targetScale;

            if (progress <= 0.7f)
            {
                float t = progress / 0.5f;
                targetScale = Mathf.Lerp(0.05f, 0.25f, t * t);
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
                float t = (progress - 0.5f) / 0.5f;
                float acceleratedT = t * t * t;
                targetScale = Mathf.Lerp(0.25f, 1.0f, acceleratedT);
            }

            if (progress > 1.0f)
            {
                float t = (progress - 1.0f) / 1.0f;
                targetScale = Mathf.Lerp(1.0f, 3.0f, t);
            }

            transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        public override void PlayAction(PatternType type)
        {
            if (type == PatternType.Signal)
            {
                PlaySfx(actionSfx); // signalSfx
                targetImage.color = Color.red;
                transform.DOScale(Vector3.one * 1.2f, 0.1f).OnComplete(() => {
                    targetImage.color = Color.white;
                    transform.DOScale(Vector3.one * 1.0f, 0.1f);
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
            targetImage.rectTransform.localRotation = Quaternion.identity;
            targetImage.color = Color.white;
        }
    }
}