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

            if (_isHolding)
            {
                if (playerHoldSlider != null)
                {
                    if (!playerHoldSlider.gameObject.activeSelf) playerHoldSlider.gameObject.SetActive(true);
                    playerHoldSlider.value = progress;
                }

                float shakeStrength = 1.5f + (progress * 4f);
                targetImage.rectTransform.anchoredPosition = new Vector2(
                    Random.Range(-shakeStrength, shakeStrength),
                    Random.Range(-shakeStrength, shakeStrength)
                );
            }
            else
            {
                targetImage.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public override void PlayAction(PatternType type)
        {
            if (_isLocked) return;

            if (type == PatternType.Hold)
            {
                _isHolding = true;
                PlaySfx(actionSfx);

                SetAnimation(actionFrames, true);
            }
        }

        public override void StopHoldAction()
        {
            if (!_isHolding) return;
            _isHolding = false;

            if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);

            SetAnimation(idleFrames, true);
        }

        public override void PlayAction(JudgeResult result)
        {
            _isHolding = false;
            _isLocked = true; 

            if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                SetAnimation(successFrames, false);
            }
            else
            {
                PlaySfx(missSfx);
                SetAnimation(missFrames, false);
            }

        }

        protected override void OnAnimationComplete()
        {
            _isLocked = false;
            base.OnAnimationComplete();
        }
    }
}