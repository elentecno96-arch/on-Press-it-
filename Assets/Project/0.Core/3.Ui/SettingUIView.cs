using DG.Tweening;
using Project.Core.Managers;
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

    private bool _isInitialSetting = false; // 초기 설정값 적용 여부 체크

    private float _lastSfxPlayTime;
    private const float SfxInterval = 0.12f;

    private void Awake()
    {
        settingsButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayUISound(UISoundType.Open);
            OnSettingsClick?.Invoke();
        });

        closeSettingsButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayUISound(UISoundType.Cancel);
            OnSettingsCloseClick?.Invoke();
        });

        resetSettingsButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayUISound(UISoundType.Check);
            OnResetSettingsClick?.Invoke();
        });

        // 2. 오디오 슬라이더 이벤트 바인딩
        bgmSlider.onValueChanged.AddListener(value => {
            if (_isInitialSetting) return;
            OnBgmVolumeChanged?.Invoke(value);
        });

        sfxSlider.onValueChanged.AddListener(value => {
            if (_isInitialSetting) return;

            OnSfxVolumeChanged?.Invoke(value);
            PlaySfxPreview();
        });

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.AddListener(isOn =>
            {
                if (_isInitialSetting) return;

                AudioManager.Instance.PlayUISound(UISoundType.Click);
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
        _isInitialSetting = true;

        bgmSlider.value = bgm;
        sfxSlider.value = sfx;

        if (vibrationToggle != null)
        {
            vibrationToggle.SetIsOnWithoutNotify(isVib);
        }

        _isInitialSetting = false; 
    }

    /// <summary>
    /// SFX 볼륨 조절 시 유저가 크기를 체감할 수 있도록 짧은 효과음을 재생합니다.
    /// </summary>
    private void PlaySfxPreview()
    {
        if (Time.time - _lastSfxPlayTime < SfxInterval) return;

        AudioManager.Instance.PlayUISound(UISoundType.Click);
        _lastSfxPlayTime = Time.time;
    }
}