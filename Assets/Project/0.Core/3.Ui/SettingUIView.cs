using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIView : MonoBehaviour
{
    // 설정 관련 이벤트
    public event Action OnSettingsClick;
    public event Action OnSettingsCloseClick;
    public event Action OnResetSettingsClick;
    public event Action<float> OnBgmVolumeChanged;
    public event Action<float> OnSfxVolumeChanged;

    [Header("--- Settings Window ---")]
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private Button settingsButton;      // 톱니바퀴
    [SerializeField] private Button closeSettingsButton; // 닫기 (X)
    [SerializeField] private Button resetSettingsButton; // 되돌리기 (리셋)
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        // 설정창 관련 버튼 이벤트 바인딩
        settingsButton.onClick.AddListener(() => OnSettingsClick?.Invoke());
        closeSettingsButton.onClick.AddListener(() => OnSettingsCloseClick?.Invoke());
        resetSettingsButton.onClick.AddListener(() => OnResetSettingsClick?.Invoke());

        // 오디오 슬라이더 이벤트 바인딩
        bgmSlider.onValueChanged.AddListener(value => OnBgmVolumeChanged?.Invoke(value));
        sfxSlider.onValueChanged.AddListener(value => OnSfxVolumeChanged?.Invoke(value));
    }

    // 설정창 활성화/비활성화
    public void ShowSettings(bool isActive) => settingsWindow.SetActive(isActive);

    // 슬라이더 값 강제 설정 (리셋 시 Presenter가 호출)
    public void SetSliderValues(float bgm, float sfx)
    {
        bgmSlider.value = bgm;
        sfxSlider.value = sfx;
    }
}