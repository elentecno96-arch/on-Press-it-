using DG.Tweening;
using Project.Core.Managers;
using Project.Rhythm.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageUiView : MonoBehaviour
{
    public event Action OnPlayClick;
    public event Action OnCloseClick;
    // 난이도 변경 이벤트를 선언합니다. (int는 방향: -1 또는 1)
    public event Action<int> OnDifficultyDirectionClicked;

    [SerializeField] private RectTransform infoPanel;
    [SerializeField] private CanvasGroup infoCanvasGroup;

    [SerializeField] private float startPositionY = -1000f; // 시작 위치
    [SerializeField] private float duration = 0.4f;

    [SerializeField] private Image stagePreviewImage;
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private float animationScale = 1.1f;
    [SerializeField] private float animationDuration = 0.15f;

    [Header("--- UI Windows ---")]
    [SerializeField] private GameObject infoWindow;

    [Header("--- Buttons ---")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;

    [Header("--- Difficulty Buttons ---")]
    // 좌우 화살표 버튼 변수
    [SerializeField] private Button LeftBtn;
    [SerializeField] private Button RightBtn;

    private bool _isActionStarted = false;

    private void Awake()
    {
        // 기존 플레이 버튼 로직
        if (playButton != null)
        {
            playButton.onClick.AddListener(() =>
            {
                if (_isActionStarted) return;
                _isActionStarted = true;
                AudioManager.Instance.PlayUISound(UISoundType.Check);
                OnPlayClick?.Invoke();
            });
        }

        // 기존 닫기 버튼 로직
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                if (_isActionStarted) return;
                AudioManager.Instance.PlayUISound(UISoundType.Cancel);
                OnCloseClick?.Invoke();
            });
        }

        // 난이도 버튼 리스너 등록
        if (LeftBtn != null)
            LeftBtn.onClick.AddListener(() => OnDifficultyDirectionClicked?.Invoke(-1));

        if (RightBtn != null)
            RightBtn.onClick.AddListener(() => OnDifficultyDirectionClicked?.Invoke(1));

        if (infoPanel != null) infoPanel.anchoredPosition = new Vector2(0, startPositionY);
        if (infoCanvasGroup != null) infoCanvasGroup.alpha = 0f;

        infoWindow.SetActive(false);
    }
    public void UpdateStageDetails(StageData data)
    {
        if (data == null) return;

        if (stagePreviewImage != null && data.stageImage != null)
        {
            stagePreviewImage.transform.DOKill();
            stagePreviewImage.transform.localScale = Vector3.one * animationScale;
            stagePreviewImage.sprite = data.stageImage;
            stagePreviewImage.transform.DOScale(1.0f, animationDuration).SetEase(Ease.OutBack);
        }

        if (stageNameText != null)
        {
            stageNameText.transform.DOComplete();
            stageNameText.text = data.stageName; 

            stageNameText.transform.localScale = Vector3.one * 1.05f;
            stageNameText.transform.DOScale(1.0f, animationDuration);
        }
    }

    // 매개변수 없이 창만 활성화하도록 수정
    public void Show()
    {
        _isActionStarted = false;
        infoWindow.SetActive(true);

        AudioManager.Instance.PlayUISound(UISoundType.Open);

        infoPanel.DOKill();
        infoCanvasGroup.DOKill();

        infoPanel.anchoredPosition = new Vector2(0, startPositionY);
        infoPanel.DOAnchorPosY(0, duration).SetEase(Ease.OutQuart); 

        if (infoCanvasGroup != null)
            infoCanvasGroup.DOFade(1f, duration * 0.5f);
    }

    public void Hide()
    {
        infoPanel.DOKill();
        infoCanvasGroup.DOKill();

        infoPanel.DOAnchorPosY(startPositionY, duration * 0.8f).SetEase(Ease.InQuart).OnComplete(() => {
            infoWindow.SetActive(false);
        });

        if (infoCanvasGroup != null)
            infoCanvasGroup.DOFade(0f, duration * 0.5f);
    }
}
