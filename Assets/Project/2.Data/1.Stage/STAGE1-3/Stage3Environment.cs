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

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip countSfx;      // 3, 2, 1
        [SerializeField] private AudioClip holdSignalSfx; // HOLD!

        private Coroutine _countCoroutine;
        private readonly string[] _countdownSteps = { "READY", "HOLD!" };
        private float _bpm = 120f;
        private float _beatDuration = 0.5f;

        private const float PULSE_FADE_SPEED = 5f; // 페이드 아웃 속도
        private readonly int _underlayDilationId = Shader.PropertyToID("_UnderlayDilation");

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

        public void SetBpm(float bpm)
        {
            if (bpm <= 0)
            {
                bpm = 120f;
            }
            _beatDuration = 60f / bpm;
        }

        public void StartCountdown(float targetBeat)
        {
            if (countdownText == null) return;
            if (_countCoroutine != null) StopCoroutine(_countCoroutine);

            countdownText.text = string.Empty;
            countdownText.transform.localScale = Vector3.one;
            _countCoroutine = StartCoroutine(TargetBeatSyncRoutine(targetBeat));
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

        private IEnumerator TargetBeatSyncRoutine(float targetBeat)
        {
            countdownText.gameObject.SetActive(true);

            float currentMusicBeat = (StageManager.CurrentTime * _bpm) / 60f;
            float beatGap = targetBeat - currentMusicBeat;

            int startIndex = 2 - Mathf.CeilToInt(beatGap);
            startIndex = Mathf.Clamp(startIndex, 0, 1);

            for (int i = startIndex; i < _countdownSteps.Length; i++)
            {
                countdownText.text = _countdownSteps[i];

                if (_countdownSteps[i] == "HOLD!")
                {
                    countdownText.color = Color.red; 
                    PlaySfx(holdSignalSfx);
                }
                else
                {
                    countdownText.color = Color.white; 
                    PlaySfx(countSfx);
                }
                Material mat = countdownText.fontMaterial;
                mat.SetFloat(_underlayDilationId, 1.0f);

                float currentStepStartBeat = targetBeat - (1 - i);
                float nextStepStartBeat = currentStepStartBeat + 1f;

                while (true)
                {
                    float nowBeat = (StageManager.CurrentTime * _bpm) / 60f;
                    if (nowBeat >= nextStepStartBeat) break;

                    float progress = Mathf.Clamp01(nowBeat - currentStepStartBeat);

                    float dilation = Mathf.Lerp(1.0f, 0.0f, progress * PULSE_FADE_SPEED);
                    mat.SetFloat(_underlayDilationId, dilation);

                    Color c = countdownText.color;
                    c.a = Mathf.Lerp(1.0f, 0.3f, progress);
                    countdownText.color = c;

                    yield return null;
                }
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