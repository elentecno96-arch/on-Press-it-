using System;
using UnityEngine;
using UnityEngine.UI;
public class MainUiView : MonoBehaviour
{
    public event Action OnPlayClick;
    [SerializeField] private PlayButtonController playButtonController; // 플레이 버튼 스크립트 할당용
    // Presenter가 구독할 이벤트
    public event Action<int> OnStageSelect;
    public event Action OnSettingsClick;
    public event Action OnSettingsCloseClick;
    public event Action OnHomeClick;

    [Header("--- Navigation Buttons ---")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button settingsButton;

    [Header("--- Settings Window ---")]
    [SerializeField] private GameObject settingsWindow; // 결과 정보와 오디오 설정이 포함된 패널
    [SerializeField] private Button closeSettingsButton;
    // [SerializeField] private Slider audioSlider; // 오디오 설정용 (필요 시 추가)

    [Header("--- Stage Info Window ---")]
    [SerializeField] private GameObject stageInfoWindow;
    [SerializeField] private Button[] stageButtons;

    private void Awake()
    {
        // 버튼 이벤트 바인딩
        homeButton.onClick.AddListener(() => OnHomeClick?.Invoke());
        settingsButton.onClick.AddListener(() => OnSettingsClick?.Invoke());
        closeSettingsButton.onClick.AddListener(() => OnSettingsCloseClick?.Invoke());

        for (int i = 0; i < stageButtons.Length; i++)
        {
            int index = i;
            if (stageButtons[index] != null)
                stageButtons[index].onClick.AddListener(() => OnStageSelect?.Invoke(index));
        }
        // 플레이 버튼 컨트롤러의 이벤트를 View의 이벤트로 연결
        if (playButtonController != null)
            playButtonController.OnPlayRequested += () => OnPlayClick?.Invoke();

        // 정보창을 끄는 버튼(돌아가기)도 추가로 연결해주면 좋습니다.
        // closeInfoButton.onClick.AddListener(() => ShowStageInfoWindow(false));
    }

    // 1. 설정창 띄우기 (결과 정보 + 오디오 포함)
    public void ShowSettings(bool isActive)
    {
        settingsWindow.SetActive(isActive);
    }

    // 2. 스테이지 정보창 제어
    public void ShowStageInfoWindow(bool isActive)
    {
        stageInfoWindow.SetActive(isActive);
    }

    // 3. 홈으로 돌아가기 (모든 팝업 닫기)
    public void ResetToMain()
    {
        settingsWindow.SetActive(false);
        stageInfoWindow.SetActive(false);
        Debug.Log("모든 창을 닫고 메인 화면으로 돌아갑니다.");
    }
}
