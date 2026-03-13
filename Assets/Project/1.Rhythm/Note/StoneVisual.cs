using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 3스테이지 돌맹이 연출
    /// </summary>
    public class StoneVisual : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite[] idleSprites;          // 대기 2프레임
        [SerializeField] private Sprite painSprite;             // 성공 직후 아파함
        [SerializeField] private Sprite[] ignoreSprites;        // 실패 시 무시함 2프레임
        [SerializeField] private Sprite[] spitSprites;          // 뱉는 입 모양 2프레임

        [SerializeField] private GameObject[] mineralPrefabs;   // 광물 랜덤 2종
        [SerializeField] private GameObject[] trashPrefabs;     // 쓰레기 랜덤 2종
        [SerializeField] private Transform mouthPos;

        private RectTransform _rectTransform;
        private bool _isJudgedn;
        private float _animTimer;
        private int _animFrame;

        private void Awake() => _rectTransform = GetComponent<RectTransform>();

        private void OnEnable()
        {
            _isJudgedn = false;
            targetImage.sprite = idleSprites[0];
            _rectTransform.localScale = Vector3.one;
        }

        public void UpdateVisual(float progress)
        {
            if (_isJudgedn) return;

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
            if (_isJudgedn) return;

            if (result != JudgeResult.Miss)
                StartCoroutine(SuccessRoutine());
            else
                StartCoroutine(FailRoutine());

            _isJudgedn = true;
        }

        private IEnumerator SuccessRoutine()
        {
            targetImage.sprite = painSprite;
            _rectTransform.localScale = new Vector3(1.1f, 0.9f, 1f);
            yield return new WaitForSeconds(0.1f);

            targetImage.sprite = spitSprites[Random.Range(0, spitSprites.Length)];
            _rectTransform.localScale = Vector3.one;
            Spit(mineralPrefabs);
        }

        private IEnumerator FailRoutine()
        {
            targetImage.sprite = ignoreSprites[0];
            yield return new WaitForSeconds(0.15f);
            targetImage.sprite = ignoreSprites[1];
            yield return new WaitForSeconds(0.15f);

            targetImage.sprite = spitSprites[Random.Range(0, spitSprites.Length)];
            Spit(trashPrefabs);
        }

        private void Spit(GameObject[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0) return;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject obj = Instantiate(prefab, mouthPos.position, Quaternion.identity, transform.parent);

            if (obj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Vector2 force = new Vector2(Random.Range(200f, 400f), Random.Range(500f, 800f));
                rb.AddForce(force);
                rb.AddTorque(Random.Range(-10f, 10f));
            }
        }

        public void PlayAction(PatternType type) { }
        public void StopHoldAction() { }
    }
}
