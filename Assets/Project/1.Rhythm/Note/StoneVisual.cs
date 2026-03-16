using Cysharp.Threading.Tasks;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 3스테이지 살아있는 돌맹이 연출 (이미지 교체 중심)
    /// </summary>
    public class StoneVisual : MonoBehaviour, ITouchVisual
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Slider holdSlider;

        [SerializeField] private Sprite[] idleSprites;    // 대기 2프레임
        [SerializeField] private Sprite painSprite;       // 성공 직후 아파함 (1프레임)
        [SerializeField] private Sprite[] ignoreSprites;  // 실패 시 무시함 2프레임
        [SerializeField] private Sprite[] spitSprites;    // 뱉는 입 모양 랜덤

        [SerializeField] private GameObject[] mineralPrefabs;
        [SerializeField] private GameObject[] trashPrefabs;
        [SerializeField] private Transform mouthPos;

        private RectTransform _rectTransform;
        private bool _isJudged = false;
        private float _animTimer;
        private int _animFrame;

        private void Awake() => _rectTransform = GetComponent<RectTransform>();

        private void OnEnable()
        {
            _isJudged = false;
            _animTimer = 0f;
            _animFrame = 0;
            targetImage.sprite = idleSprites[0];
            _rectTransform.localScale = Vector3.one;
            _rectTransform.anchoredPosition = Vector2.zero;

            if (holdSlider != null) holdSlider.gameObject.SetActive(false);
        }

        public void UpdateVisual(float progress)
        {
            if (progress <= 0.01f && _isJudged)
            {
                ResetVisual();
            }

            if (_isJudged) return;

            if (progress > 0 && progress < 1.0f)
            {
                if (holdSlider != null)
                {
                    if (!holdSlider.gameObject.activeSelf) holdSlider.gameObject.SetActive(true);
                    holdSlider.value = progress;
                }

                float shake = Mathf.Sin(Time.time * 50f) * (progress * 5f);
                _rectTransform.anchoredPosition = new Vector2(shake, 0);
            }
            else if (progress >= 1.0f || progress <= 0)
            {
                if (holdSlider != null && holdSlider.gameObject.activeSelf)
                    holdSlider.gameObject.SetActive(false);
            }

            _animTimer += Time.deltaTime;
            if (_animTimer >= 0.2f)
            {
                _animTimer = 0f;
                _animFrame = (_animFrame + 1) % idleSprites.Length;
                targetImage.sprite = idleSprites[_animFrame];
            }
        }

        public void PlayAction(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            if (holdSlider != null) holdSlider.gameObject.SetActive(false);

            if (result != JudgeResult.Miss)
            {
                SuccessRoutine().Forget();
            }
            else
            {
                FailRoutine().Forget();
            }
        }

        private async UniTask SuccessRoutine()
        {
            targetImage.sprite = painSprite;

            if (spitSprites != null && spitSprites.Length > 0)
            {
                foreach (var s in spitSprites)
                {
                    targetImage.sprite = s;
                    await UniTask.Delay(100);
                }
            }

            Spit(mineralPrefabs);

            await UniTask.Delay(1000);
            ResetVisual();
        }

        private async UniTask FailRoutine()
        {
            targetImage.sprite = ignoreSprites[0];
            Spit(trashPrefabs); 
            await UniTask.Delay(1000);
            ResetVisual();
        }

        private void Spit(GameObject[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0) return;
            GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
            Instantiate(prefab, mouthPos.position, Quaternion.identity, transform.parent);
        }

        public void ResetVisual()
        {
            _isJudged = false;
            _animTimer = 0f;
            targetImage.sprite = idleSprites[0];
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = Vector3.one;
        }

        public void PlayAction(PatternType type) { }
        public void StopHoldAction() { }

        public void StartCountdown(float duration) { }
    }
}