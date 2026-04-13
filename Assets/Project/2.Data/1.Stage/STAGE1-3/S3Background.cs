using System.Collections;
using Project.Core.Managers;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using TMPro;
using UnityEngine;

namespace Project.Data.Stage.STAGE3
{
    public class S3Background : BaseRhythmVisual
    {
        [Header("Stage 3 Custom View")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private AudioClip holdSignalSfx;

        private Coroutine _countCoroutine;
        private readonly string[] _countdownSteps = { "READY", "HOLD!" };

        private const float PULSE_FADE_SPEED = 5f;
        private readonly int _underlayDilationId = Shader.PropertyToID("_UnderlayDilation");

        protected override void Awake()
        {
            base.Awake();
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
                countdownText.text = string.Empty;
            }
        }

        public override void StartCountdown(float targetBeat)
        {
            if (countdownText == null) return;
            if (_countCoroutine != null) StopCoroutine(_countCoroutine);

            if (_bpm <= 0) _bpm = 120f;

            countdownText.text = string.Empty;
            countdownText.transform.localScale = Vector3.one;

            _countCoroutine = StartCoroutine(TargetBeatSyncRoutine(targetBeat));
        }

        private IEnumerator TargetBeatSyncRoutine(float targetBeat)
        {
            countdownText.gameObject.SetActive(true);
            Material textMat = countdownText.fontMaterial;
            int lastStepIndex = -1;

            while (true)
            {
                float currentTime = CurrentTime;
                float currentMusicBeat = (currentTime * _bpm) / 60f;

                float beatGap = targetBeat - currentMusicBeat;

                if (beatGap <= 0f) break;

                int stepIndex = -1;
                if (beatGap <= 1.0f) stepIndex = 1;      // HOLD! (타겟 1비트 전부터 타겟까지)
                else if (beatGap <= 2.0f) stepIndex = 0; // READY (타겟 2비트 전부터 1비트 전까지)

                if (stepIndex == -1)
                {
                    yield return null;
                    continue;
                }

                if (stepIndex != lastStepIndex)
                {
                    lastStepIndex = stepIndex;
                    countdownText.text = _countdownSteps[stepIndex];
                    textMat.SetFloat(_underlayDilationId, 1.0f);

                    if (stepIndex == 1) // HOLD!
                    {
                        countdownText.color = Color.red;
                        if (holdSignalSfx != null) PlaySfx(holdSignalSfx);
                    }
                    else // READY
                    {
                        countdownText.color = Color.white;
                        if (countSfx != null) PlaySfx(countSfx);
                    }
                }

                float progress = 1.0f - (stepIndex == 0 ? (beatGap - 1.0f) : beatGap);
                progress = Mathf.Clamp01(progress);

                float dilation = Mathf.Lerp(1.0f, 0.0f, progress * PULSE_FADE_SPEED);
                textMat.SetFloat(_underlayDilationId, dilation);

                Color c = countdownText.color;
                c.a = Mathf.Lerp(1.0f, 0.3f, progress);
                countdownText.color = c;

                yield return null;
            }

            countdownText.gameObject.SetActive(false);
            _countCoroutine = null;
        }

        public override void PlayAction(PatternType type) { }
        public override void PlayAction(JudgeResult result) { }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_countCoroutine != null)
            {
                StopCoroutine(_countCoroutine);
                _countCoroutine = null;
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}