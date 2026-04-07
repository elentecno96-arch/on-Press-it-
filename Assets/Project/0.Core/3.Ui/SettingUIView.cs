using DG.Tweening;
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
    public event Action<bool> OnVibrationChanged;

    [SerializeField] private CanvasGroup settingsCanvasGroup; 
    [SerializeField] private RectTransform settingsPanel;

    [Header("--- Settings Window ---")]
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private Button settingsButton;      // 톱니바퀴
    [SerializeField] private Button closeSettingsButton; // 닫기 (X)
    [SerializeField] private Button resetSettingsButton; // 되돌리기 (리셋)
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle vibrationToggle;

    private void Awake()
    {
        // 설정창 관련 버튼 이벤트 바인딩
        settingsButton.onClick.AddListener(() => OnSettingsClick?.Invoke());
        closeSettingsButton.onClick.AddListener(() => OnSettingsCloseClick?.Invoke());
        resetSettingsButton.onClick.AddListener(() => OnResetSettingsClick?.Invoke());

        // 오디오 슬라이더 이벤트 바인딩
        bgmSlider.onValueChanged.AddListener(value => OnBgmVolumeChanged?.Invoke(value));
        sfxSlider.onValueChanged.AddListener(value => OnSfxVolumeChanged?.Invoke(value));

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.AddListener(isOn =>
            {
                OnVibrationChanged?.Invoke(isOn);

                if (isOn)
                {
                    #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                    #endif
                }
            });
        }
        if (settingsPanel != null) settingsPanel.localScale = Vector3.zero;
        if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = 0f;
        settingsWindow.SetActive(false);
    }

    // 설정창 활성화/비활성화
    public void ShowSettings(bool isActive)
    {
        // 모든 트윈 중복 실행 방지
        settingsPanel.DOKill();
        settingsCanvasGroup.DOKill();

        if (isActive)
        {
            settingsWindow.SetActive(true);

            settingsPanel.localScale = Vector3.one * 0.8f;
            settingsPanel.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack); 

            if (settingsCanvasGroup != null)
                settingsCanvasGroup.DOFade(1f, 0.2f);
        }
        else
        {
            settingsPanel.DOScale(0.8f, 0.2f).SetEase(Ease.InBack);

            if (settingsCanvasGroup != null)
            {
                settingsCanvasGroup.DOFade(0f, 0.2f).OnComplete(() => {
                    settingsWindow.SetActive(false);
                });
            }
            else
            {
                settingsWindow.SetActive(false);
            }
        }
    }

    public void SetSettingValues(float bgm, float sfx, bool isVib)
    {
        bgmSlider.value = bgm;
        sfxSlider.value = sfx;

        if (vibrationToggle != null)
        {
            vibrationToggle.SetIsOnWithoutNotify(isVib);
        }
    }
}