using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageUiView : MonoBehaviour
{
    public event Action OnPlayClick;
    public event Action OnCloseClick;
    // 난이도 변경 이벤트를 선언합니다. (int는 방향: -1 또는 1)
    public event Action<int> OnDifficultyDirectionClicked;

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

                OnPlayClick?.Invoke();
            });
        }

        // 기존 닫기 버튼 로직
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                if (_isActionStarted) return;

                OnCloseClick?.Invoke();
            });
        }

        // 난이도 버튼 리스너 등록
        if (LeftBtn != null)
            LeftBtn.onClick.AddListener(() => OnDifficultyDirectionClicked?.Invoke(-1));

        if (RightBtn != null)
            RightBtn.onClick.AddListener(() => OnDifficultyDirectionClicked?.Invoke(1));

        Hide();
        Debug.Log("[StageUiView] 초기화 완료. 창이 숨겨졌습니다.1");
    }

    // 매개변수 없이 창만 활성화하도록 수정
    public void Show()
    {
        _isActionStarted = false;

        if (infoWindow != null)
            infoWindow.SetActive(true);
    }

    public void Hide()
    {
        if (infoWindow != null)
            infoWindow.SetActive(false);
        Debug.Log("[StageUiView] 창이 숨겨졌습니다.");
    }   
}
