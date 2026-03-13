using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 스테이지 2 박쥐 노트 
    /// </summary>
    public class BatVisual : MonoBehaviour, ITouchVisual
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite[] eyeSprites;      // 빨간 눈 랜덤 5종
        [SerializeField] private Sprite[] batSprites;      // 박쥐 랜덤 2종
        [SerializeField] private Sprite[] successSprites;  // 성공 이미지 랜덤 2종
        [SerializeField] private Sprite[] missSprites;     // 실패 이미지 랜덤 2종

        [SerializeField] private float scaleUpStart = 0.6f;
        [SerializeField] private float fallSpeed = 2500f;

        private RectTransform _rectTransform;
        private bool _isJudged;
        private bool _isFalling;
        private bool _isBatSpawned; 

        private Sprite _selectedBatSprite;
        private Vector2 _fallDirection = new Vector2(-0.4f, -1.2f).normalized;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _isJudged = false;
            _isFalling = false;
            _isBatSpawned = false;

            if (eyeSprites.Length > 0 && targetImage != null)
            {
                targetImage.sprite = eyeSprites[Random.Range(0, eyeSprites.Length)];
                targetImage.color = Color.white;
            }

            if (batSprites.Length > 0)
                _selectedBatSprite = batSprites[Random.Range(0, batSprites.Length)];

            _rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);
            _rectTransform.localRotation = Quaternion.identity;
        }

        public void UpdateVisual(float progress)
        {
            if (_isFalling)
            {
                _rectTransform.anchoredPosition += _fallDirection * fallSpeed * Time.deltaTime;
                _rectTransform.Rotate(Vector3.forward * 600f * Time.deltaTime);
                return;
            }

            if (!_isJudged)
            {
                if (progress < scaleUpStart)
                {
                    _rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);
                }
                else if (progress >= scaleUpStart)
                {
                    if (!_isBatSpawned)
                    {
                        _isBatSpawned = true;
                        targetImage.sprite = _selectedBatSprite; 
                    }
                    float t = (progress - scaleUpStart) / (1.0f - scaleUpStart);
                    float currentScale = Mathf.Lerp(0.4f, 1.0f, t);
                    _rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
                }
            }
        }

        public void PlayAction(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            if (result != JudgeResult.Miss)
            {
                if (successSprites.Length > 0)
                    targetImage.sprite = successSprites[Random.Range(0, successSprites.Length)];

                _isFalling = true;
            }
            else
            {
                if (missSprites.Length > 0)
                    targetImage.sprite = missSprites[Random.Range(0, missSprites.Length)];

                targetImage.color = Color.red;
                Destroy(gameObject, 0.15f);
            }
        }

        public void PlayAction(PatternType type) { }
        public void StopHoldAction() { }
    }
}