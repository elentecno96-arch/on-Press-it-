using Cysharp.Threading.Tasks;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
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
            if (_isJudged) return;
            _isJudged = true;

            if (holdSlider != null) holdSlider.gameObject.SetActive(false);

            if (result != JudgeResult.Miss)
            {
                PlaySfx(successSfx);
                SuccessRoutine().Forget();
            }
            else
            {
                PlaySfx(missSfx);
                FailRoutine().Forget();
            }
        }

        private async UniTask SuccessRoutine()
        {
            targetImage.sprite = painSprite;
            if (actionFrames != null && actionFrames.Length > 0)
            {
                foreach (var s in actionFrames)
                {
                    targetImage.sprite = s;
                    await UniTask.Delay(100);
                }
            }

            PlaySfx(spitSfx);
            Spit(mineralPrefabs);

            await UniTask.Delay(1000);
            ResetVisual();
        }

        private async UniTask FailRoutine()
        {
            if (missFrames.Length > 0) targetImage.sprite = missFrames[0];
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

        public override void PlayAction(PatternType type) { }
    }
}