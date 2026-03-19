using System;
using UnityEngine;
using UnityEngine.UI;

public class StageUiView : MonoBehaviour
{
    public event Action OnPlayClick;
    public event Action OnCloseClick;

    [Header("--- UI Windows ---")]
    [SerializeField] private GameObject infoWindow;

    [Header("--- Buttons ---")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(() => OnPlayClick?.Invoke());

        if (closeButton != null)
            closeButton.onClick.AddListener(() => OnCloseClick?.Invoke());

        Hide();
    }

    // 매개변수 없이 창만 활성화하도록 수정
    public void Show()
    {
        if (infoWindow != null)
            infoWindow.SetActive(true);
    }

    public void Hide()
    {
        if (infoWindow != null)
            infoWindow.SetActive(false);
    }
}
