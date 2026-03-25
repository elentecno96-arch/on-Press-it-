using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageUiView : MonoBehaviour
{
    public event Action OnPlayClick;
    public event Action OnCloseClick;

    [Header("--- UI Windows ---")]
    [SerializeField] private GameObject infoWindow;

    [Header("--- Buttons ---")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;    

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

        Hide();
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
    }   
}
