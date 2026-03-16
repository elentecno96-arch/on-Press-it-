using System.Collections;
using TMPro;
using UnityEngine;
using Project.Core.Managers; // AudioManager 참조

namespace Project.Data.Stage.STAGE3
{
    /// <summary>
    /// 스테이지 3 배경 전용 스크립트: 자동 카운트다운 가이드 및 사운드 담당
    /// </summary>
    public class Stage3Environment : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;

        [SerializeField] private float stepDuration = 0.75f;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip countSfx;      // 3, 2, 1
        [SerializeField] private AudioClip holdSignalSfx; // HOLD!

        private Coroutine _countCoroutine;
        private readonly string[] _countdownSteps = { "3", "2", "1", "HOLD!" };

        private void Awake()
        {

            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.AssignMixerGroup(audioSource);
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
                countdownText.text = string.Empty;
            }
        }

        public void StartCountdown()
        {
            if (countdownText == null) return;
            if (_countCoroutine != null) StopCoroutine(_countCoroutine);
            _countCoroutine = StartCoroutine(CountdownRoutine());
        }

        public void StopCountdown()
        {
            if (_countCoroutine != null)
            {
                StopCoroutine(_countCoroutine);
                _countCoroutine = null;
            }

            if (countdownText != null)
            {
                countdownText.text = string.Empty;
                countdownText.gameObject.SetActive(false);
            }
        }

        private IEnumerator CountdownRoutine()
        {
            countdownText.gameObject.SetActive(true);

            foreach (var step in _countdownSteps)
            {
                countdownText.text = step;

                if (step == "HOLD!")
                {
                    countdownText.color = Color.red;
                    PlaySfx(holdSignalSfx); 
                }
                else
                {
                    countdownText.color = Color.white;
                    PlaySfx(countSfx);      
                }

                float elapsed = 0f;
                float animDuration = stepDuration * 0.7f;
                float waitDuration = stepDuration * 0.3f;

                while (elapsed < animDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / animDuration;

                    countdownText.transform.localScale = Vector3.Lerp(Vector3.one * 1.6f, Vector3.one, progress);
                    Color c = countdownText.color;
                    c.a = Mathf.Lerp(0.5f, 1f, progress);
                    countdownText.color = c;

                    yield return null;
                }

                yield return new WaitForSeconds(waitDuration);
            }

            countdownText.gameObject.SetActive(false);
            _countCoroutine = null;
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDisable()
        {
            StopCountdown();
        }
    }
}