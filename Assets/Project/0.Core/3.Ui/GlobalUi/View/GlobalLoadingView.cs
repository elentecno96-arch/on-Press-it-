using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLoadingView : MonoBehaviour
{
    private const float DEFAULT_MOVE_DISTANCE = 500f;   // 이동 연출 거리
    private const float DEFAULT_TWEEN_DURATION = 0.4f;  // 연출 기본 시간
    private const float PROGRESS_ZERO = 0f;             // 초기 게이지 값

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private RectTransform loadingBarRoot;

    [SerializeField] private float moveDistance = DEFAULT_MOVE_DISTANCE;
    [SerializeField] private float tweenDuration = DEFAULT_TWEEN_DURATION;

    private Vector2 _originalPos;
    private Tween _progressTween;

    public void Init()
    {
        _originalPos = loadingBarRoot.anchoredPosition;
        loadingBar.value = PROGRESS_ZERO;
        loadingText.text = string.Empty;

        if (canvasGroup != null) canvasGroup.alpha = 0;
        loadingBarRoot.localScale = Vector3.one * 0.8f;

        SetVisible(false);
    }

    /// <summary>
    /// 캔버스 활성화/비활성화
    /// </summary>
    /// <param name="isActive"></param>
    public void SetVisible(bool isActive)
    {
        gameObject.SetActive(isActive);

        if (loadingCanvas != null)
        {
            loadingCanvas.enabled = isActive;
        }
    }

    public void SetText(string message) => loadingText.text = message; //SO로 관리 예정

    public async UniTask Show()
    {
        canvasGroup.alpha = 0;
        loadingBarRoot.localScale = Vector3.one * 0.9f;

        await UniTask.WhenAll(
            canvasGroup.DOFade(1f, tweenDuration).ToUniTask(),
            loadingBarRoot.DOScale(1f, tweenDuration).SetEase(Ease.OutBack).ToUniTask()
        );
    }

    public async UniTask Hide()
    {
        _progressTween?.Kill();

        await UniTask.WhenAll(
            canvasGroup.DOFade(0f, tweenDuration).ToUniTask(),
            loadingBarRoot.DOScale(0.9f, tweenDuration).SetEase(Ease.InQuad).ToUniTask()
        );

        Debug.Log("Hide 연출 완료");
    }

    public async UniTask UpdateProgress(float value, float duration)
    {
        if (_progressTween != null && _progressTween.IsActive())
        {
            _progressTween.Kill(false);
        }

        if (duration <= 0.01f)
        {
            loadingBar.value = value;
            return;
        }

        _progressTween = loadingBar.DOValue(value, duration).SetEase(Ease.Linear);

        await _progressTween.ToUniTask(TweenCancelBehaviour.Kill, this.GetCancellationTokenOnDestroy());
    }
}
