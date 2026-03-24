using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Rhythm.Player
{
    public class Stage3PlayerVisual : BaseRhythmVisual
    {
        [SerializeField] private Slider playerHoldSlider;
        private bool _isLocked;

        protected override void Awake()
        {
            base.Awake();
            if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);
        }
        protected override void Update()
        {
            base.Update();
        }

        public override void UpdateVisual(float progress)
        {
            if (_isLocked) return;

            if (progress > 0f && progress < 1.0f)
            {
                _isHolding = true;

                if (playerHoldSlider != null)
                {
                    if (!playerHoldSlider.gameObject.activeSelf) playerHoldSlider.gameObject.SetActive(true);
                    playerHoldSlider.value = progress;
                }

                float shakeStrength = 2f + (progress * 6f);
                targetImage.rectTransform.anchoredPosition = new Vector2(
                    Random.Range(-shakeStrength, shakeStrength),
                    Random.Range(-shakeStrength, shakeStrength)
                );

                if (_currentAnimation != actionFrames)
                {
                    SetAnimation(actionFrames, actionFrameRate, true);
                }
            }
        }

        public override void PlayAction(PatternType type)
        {
            if (_isLocked) return;

            if (type == PatternType.Hold)
            {
                _isHolding = true;
                PlaySfx(actionSfx);

                SetAnimation(actionFrames, actionFrameRate, true);
            }
        }

        public override void StopHoldAction()
        {
            _isHolding = false;
            if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);
            targetImage.rectTransform.anchoredPosition = Vector2.zero;

            if (!_isLocked) SetAnimation(idleFrames, idleFrameRate, true);
        }

        public override void PlayAction(JudgeResult result)
        {
            _isHolding = false;
            _isLocked = true;

            if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);
            targetImage.rectTransform.anchoredPosition = Vector2.zero;

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                SetAnimation(successFrames, successFrameRate, false);
            }
            else
            {
                PlaySfx(missSfx);
                SetAnimation(missFrames, missFrameRate, false);
            }
        }

        public override void ResetVisual()
        {
            _isJudged = false;
            _isLocked = false;
            _isHolding = false;
            if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);
            targetImage.rectTransform.anchoredPosition = Vector2.zero;
            SetAnimation(idleFrames, idleFrameRate, true);
            base.ResetVisual();
        }

        protected override void OnAnimationComplete()
        {
            _isLocked = false;
            _isJudged = false;
            base.OnAnimationComplete();
        }
    }
}