using Cysharp.Threading.Tasks;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 3스테이지 살아있는 돌맹이 연출 (홀드 대응 버전)
    /// </summary>
    public class StoneVisual : MonoBehaviour, ITouchVisual
    {
        [Header("UI & Base")]
        [SerializeField] private Image targetImage;
        [SerializeField] private Slider holdSlider;       // [추가] 홀드 게이지용 슬라이더

        [Header("Sprites")]
        [SerializeField] private Sprite[] idleSprites;    // 대기 2프레임
        [SerializeField] private Sprite painSprite;       // 성공 직후 아파함
        [SerializeField] private Sprite[] ignoreSprites;  // 실패 시 무시함 2프레임
        [SerializeField] private Sprite[] spitSprites;    // 뱉는 입 모양 2프레임

        [Header("Effects")]
        [SerializeField] private GameObject[] mineralPrefabs;
        [SerializeField] private GameObject[] trashPrefabs;
        [SerializeField] private Transform mouthPos;

        private RectTransform _rectTransform;
        private bool _isJudged;
        private float _animTimer;
        private int _animFrame;

        private void Awake() => _rectTransform = GetComponent<RectTransform>();

        private void OnEnable()
        {
            _isJudged = false;
            targetImage.sprite = idleSprites[0];
            _rectTransform.localScale = Vector3.one;

            // 시작 시 슬라이더 숨기기
            if (holdSlider != null) holdSlider.gameObject.SetActive(false);
        }

        public void UpdateVisual(float progress)
        {
            if (_isJudged) return;

            if (progress > 0 && progress < 1.0f)
            {
                if (holdSlider != null)
                {
                    holdSlider.gameObject.SetActive(true);
                    holdSlider.value = progress;
                }

                float shake = Mathf.Sin(Time.time * 50f) * (progress * 5f);
                _rectTransform.anchoredPosition = new Vector2(shake, 0);
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
            _rectTransform.localScale = new Vector3(1.1f, 0.8f, 1f);

            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            targetImage.sprite = spitSprites[UnityEngine.Random.Range(0, spitSprites.Length)];
            _rectTransform.localScale = Vector3.one;
            Spit(mineralPrefabs);

            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
            ResetVisual();
        }

        private async UniTask FailRoutine()
        {
            targetImage.sprite = ignoreSprites[0];
            // yield return new WaitForSeconds(0.15f) 대신 await 사용
            await UniTask.Delay(TimeSpan.FromSeconds(0.15f));

            targetImage.sprite = ignoreSprites[1];
            await UniTask.Delay(TimeSpan.FromSeconds(0.15f));

            targetImage.sprite = spitSprites[UnityEngine.Random.Range(0, spitSprites.Length)];
            Spit(trashPrefabs);

            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
            ResetVisual();
        }

        private void Spit(GameObject[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0) return;
            GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
            GameObject obj = Instantiate(prefab, mouthPos.position, Quaternion.identity, transform.parent);

            if (obj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Vector2 force = new Vector2(UnityEngine.Random.Range(200f, 400f), UnityEngine.Random.Range(500f, 800f));
                rb.AddForce(force);
                rb.AddTorque(UnityEngine.Random.Range(-10f, 10f));
            }
        }

        public void ResetVisual()
        {
            _isJudged = false;
            targetImage.sprite = idleSprites[0];
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = Vector3.one;
        }

        public void PlayAction(PatternType type) { }
        public void StopHoldAction() { }
    }
}