using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLoadingView : MonoBehaviour
{
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private RectTransform loadingBarRoot;

    [SerializeField] private float moveDistance = 500f;
    [SerializeField] private float tweenDuration = 0.4f;

    private Vector2 _originalPos;
    private Tween _progressTween;

    public void Init()
    {
        _originalPos = loadingBarRoot.anchoredPosition;
        loadingBar.value = 0f;
        loadingText.text = string.Empty;
        SetVisible(false);
    }

    public void SetVisible(bool isActive) => loadingCanvas.enabled = isActive;
    public void SetText(string message) => loadingText.text = message;

    public async UniTask Show()
    {
        loadingBarRoot.anchoredPosition = new Vector2(_originalPos.x - moveDistance, _originalPos.y);
        await loadingBarRoot.DOAnchorPos(_originalPos, tweenDuration).SetEase(Ease.OutBack).ToUniTask();
    }

    public async UniTask Hide()
    {
        Vector2 targetPos = new Vector2(_originalPos.x + moveDistance, _originalPos.y);
        await loadingBarRoot.DOAnchorPos(targetPos, tweenDuration)
                            .SetEase(Ease.InBack)
                            .ToUniTask();

        loadingBarRoot.anchoredPosition = _originalPos;
    }

    public async UniTask UpdateProgress(float value, float duration)
    {
        _progressTween?.Kill();

        if (duration <= 0f)
        {
            loadingBar.value = value;
            return;
        }

        _progressTween = loadingBar.DOValue(value, duration);
        await _progressTween.ToUniTask();
    }
}
