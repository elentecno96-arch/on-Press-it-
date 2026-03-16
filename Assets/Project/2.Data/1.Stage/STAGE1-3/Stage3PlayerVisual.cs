using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stage3PlayerVisual : MonoBehaviour, ITouchVisual
{
    [SerializeField] private Image handImage;
    [SerializeField] private Slider playerHoldSlider;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private Sprite[] idleSprites;    
    [SerializeField] private Sprite[] holdSprites;    
    [SerializeField] private Sprite[] successSprites; 
    [SerializeField] private Sprite[] failSprites;

    private Coroutine _countdownCoroutine;

    private float _animTimer;
    private int _animFrame;
    private bool _isHolding;
    private bool _isLocked;

    private void Start()
    {
        if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_isLocked || _isHolding) return;

        _animTimer += Time.deltaTime;
        if (_animTimer >= 0.15f)
        {
            _animTimer = 0f;
            if (idleSprites != null && idleSprites.Length > 0)
            {
                _animFrame = (_animFrame + 1) % idleSprites.Length;
                handImage.sprite = idleSprites[_animFrame];
            }
        }
    }

    public void PlayAction(PatternType type)
    {
        if (_isLocked) return;

        if (type == PatternType.Hold)
        {
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            if (countdownText != null) countdownText.gameObject.SetActive(false);

            _isHolding = true;
            _animFrame = 0;
            if (holdSprites.Length > 0) handImage.sprite = holdSprites[0];
        }
    }

    public void StopHoldAction()
    {
        if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);
    }

    public void PlayAction(JudgeResult result)
    {
        _isHolding = false;
        StopAllCoroutines();
        StartCoroutine(JudgeResultRoutine(result));
    }

    private IEnumerator JudgeResultRoutine(JudgeResult result)
    {
        _isLocked = true;
        if (playerHoldSlider != null) playerHoldSlider.gameObject.SetActive(false);

        Sprite[] targetSprites = (result != JudgeResult.Miss) ? successSprites : failSprites;

        if (targetSprites != null && targetSprites.Length >= 2)
        {
            handImage.sprite = targetSprites[0];
            yield return new WaitForSeconds(0.08f);

            handImage.sprite = targetSprites[1]; 
            yield return new WaitForSeconds(0.6f);
        }

        Unlock();
    }

    private void Unlock()
    {
        _isLocked = false;
        _isHolding = false;
        _animTimer = 0f;
        _animFrame = 0;
    }

    public void UpdateVisual(float progress)
    {
        if (_isLocked) return;

        if (progress > 0 && progress < 1.0f && !_isHolding)
        {
            _isHolding = true;
        }

        if (_isHolding)
        {
            if (playerHoldSlider != null)
            {
                if (!playerHoldSlider.gameObject.activeSelf) playerHoldSlider.gameObject.SetActive(true);
                playerHoldSlider.value = progress;
            }
            float shakeStrength = 1.5f + (progress * 4f);
            float shakeX = Random.Range(-shakeStrength, shakeStrength);
            float shakeY = Random.Range(-shakeStrength, shakeStrength);
            handImage.rectTransform.anchoredPosition = new Vector2(shakeX, shakeY);

            UpdateHoldAnimation();
        }
        else
        {
            handImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateHoldAnimation()
    {
        if (holdSprites == null || holdSprites.Length == 0) return;

        _animTimer += Time.deltaTime;
        if (_animTimer >= 0.12f)
        {
            _animTimer = 0f;
            _animFrame = (_animFrame + 1) % holdSprites.Length;
            handImage.sprite = holdSprites[_animFrame];
        }
    }

    public void StartCountdown(float duration)
    {
        if (countdownText == null) return;

        // 강제로 텍스트 오브젝트를 켜고 루틴 시작
        countdownText.gameObject.SetActive(true);
        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        string[] counts = { "3", "2", "1", "GO!" };

        foreach (var c in counts)
        {
            countdownText.text = c;
            countdownText.transform.localScale = Vector3.one * 1.5f;

            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                countdownText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t * 2);
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
        }

        countdownText.gameObject.SetActive(false);
    }
}
