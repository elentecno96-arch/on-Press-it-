using Cysharp.Threading.Tasks;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Rhythm.Note
{
    public class StoneVisual : BaseRhythmVisual
    {
        [SerializeField] private Slider holdSlider;
        [SerializeField] private Sprite painSprite;             // 성공 직후 아파함
        [SerializeField] private GameObject[] mineralPrefabs;
        [SerializeField] private GameObject[] trashPrefabs;
        [SerializeField] private Transform mouthPos;
        [SerializeField] private AudioClip spitSfx;             // 광물 튀어나오는 소리

        private CancellationTokenSource _routineCts;

        public override void UpdateVisual(float progress)
        {
            if (_isJudged) return;

            if (progress > 0 && progress < 1.0f)
            {
                if (holdSlider != null)
                {
                    if (!holdSlider.gameObject.activeSelf) holdSlider.gameObject.SetActive(true);
                    holdSlider.value = progress;
                }

                float shake = Mathf.Sin(Time.time * 50f) * (progress * 5f);
                targetImage.rectTransform.anchoredPosition = new Vector2(shake, 0);
            }
            else
            {
                if (holdSlider != null) holdSlider.gameObject.SetActive(false);
                targetImage.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public override void PlayAction(JudgeResult result)
        {
            CancelCurrentRoutine();

            var token = RefreshToken();
            _isJudged = true;

            if (holdSlider != null) holdSlider.gameObject.SetActive(false);

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                SuccessRoutine(token).Forget();
            }
            else
            {
                PlaySfx(missSfx);
                FailRoutine(token).Forget();
            }
        }

        private async UniTask SuccessRoutine(CancellationToken token)
        {
            targetImage.sprite = painSprite;

            bool isCanceled = await UniTask.Delay(1000, cancellationToken: token).SuppressCancellationThrow();
            if (isCanceled) return; 

            if (actionFrames != null && actionFrames.Length > 0)
            {
                foreach (var s in actionFrames)
                {
                    targetImage.sprite = s;
                    isCanceled = await UniTask.Delay((int)(actionFrameRate * 500), cancellationToken: token).SuppressCancellationThrow();
                    if (isCanceled) return;
                }
            }

            PlaySfx(spitSfx);
            Spit(mineralPrefabs);

            isCanceled = await UniTask.Delay(1000, cancellationToken: token).SuppressCancellationThrow();
            if (isCanceled) return;

            ResetVisual();
        }

        private async UniTask FailRoutine(CancellationToken token)
        {
            if (missFrames.Length > 0) targetImage.sprite = missFrames[0];
            Spit(trashPrefabs);
            await UniTask.Delay(1000);

            bool isCanceled = await UniTask.Delay(1000, cancellationToken: token).SuppressCancellationThrow();
            if (isCanceled) return;

            ResetVisual();
        }

        private void CancelCurrentRoutine()
        {
            if (_routineCts != null)
            {
                _routineCts.Cancel();
                _routineCts.Dispose();
                _routineCts = null;
            }
        }

        public override void ResetVisual()
        {
            CancelCurrentRoutine();
            _isJudged = false;
            targetImage.rectTransform.anchoredPosition = Vector2.zero;
            base.ResetVisual();
        }

        private void Spit(GameObject[] prefabs)
        {
            if (!gameObject.activeInHierarchy) return;
            if (prefabs == null || prefabs.Length == 0) return;
            GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
            Instantiate(prefab, mouthPos.position, Quaternion.identity, transform.parent);
        }

        protected override void OnDisable()
        {
            base.OnDisable(); 

            if (holdSlider != null) holdSlider.gameObject.SetActive(false);
            targetImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void OnDestroy()
        {
            CancelCurrentRoutine();
        }

        public override void PlayAction(PatternType type) { }
    }
}