using Project.Core.Managers;
using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using System.Collections.Generic;
using UnityEngine;

public class MainUiPresenter : MonoBehaviour
{
    [Header("--- Views ---")]
    [SerializeField] private SettingUIView _settingView;
    [SerializeField] private StageUiView _stageView;

    [Header("--- Stage Slots ---")]
    [SerializeField] private List<StageSlot> _stageSlots;
    [SerializeField] private AudioClip _mainBgmClip;

    private StageData _currentSelectedStage;
    private const float DefaultVolume = 0.5f;

    private void Start()
    {
        // 시작 시 AudioManager에 저장된 값을 UI 슬라이더에 세팅
        if (AudioManager.Instance != null && _settingView != null)
        {
            _settingView.SetSliderValues(
                AudioManager.Instance.BgmVolume,
                AudioManager.Instance.SfxVolume
            );
        }

        // 배경음 재생
        if (_mainBgmClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(_mainBgmClip);
        }
    }

    private void OnEnable()
    {
        _settingView.OnSettingsClick += () => _settingView.ShowSettings(true);
        _settingView.OnSettingsCloseClick += () => _settingView.ShowSettings(false);

        _settingView.OnBgmVolumeChanged += (vol) => AudioManager.Instance.SetVolume("BGM", vol);
        _settingView.OnSfxVolumeChanged += (vol) => AudioManager.Instance.SetVolume("SFX", vol);
        _settingView.OnResetSettingsClick += HandleResetSettings;
        // [추가] 씬이 시작될 때 AudioManager의 현재 값을 UI에 반영
        SyncUiWithAudio();

        foreach (var slot in _stageSlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked += HandleStageSelected;
            }
        }

        _stageView.OnPlayClick += HandlePlayGame;
        _stageView.OnCloseClick += () => _stageView.Hide();
    }
    // [추가] 씬이 시작될 때 AudioManager의 현재 값을 UI에 반영
    private void SyncUiWithAudio()
    {
        if (AudioManager.Instance != null && _settingView != null)
        {
            _settingView.SetSliderValues(
                AudioManager.Instance.BgmVolume,
                AudioManager.Instance.SfxVolume
            );
        }

    }
    private void OnDisable()
    {
        if (_settingView != null)
            _settingView.OnResetSettingsClick -= HandleResetSettings;

        if (_stageSlots != null)
        {
            foreach (var slot in _stageSlots)
            {
                if (slot != null) slot.OnSlotClicked -= HandleStageSelected;
            }
        }

        if (_stageView != null)
        {
            _stageView.OnPlayClick -= HandlePlayGame;
        }

        _settingView.OnSettingsCloseClick += () =>
        {
            // 1. 설정창을 닫습니다.
            _settingView.ShowSettings(false);

            // 2. [추가] 오디오 매니저에게 저장 신호를 보내라고 명령합니다.
            AudioManager.Instance.AudioSaveSettings();
        };
    }

    private void HandleResetSettings()
    {
        AudioManager.Instance.SetVolume("BGM", DefaultVolume);
        AudioManager.Instance.SetVolume("SFX", DefaultVolume);
        _settingView.SetSliderValues(DefaultVolume, DefaultVolume);
    }

    private void HandleStageSelected(StageData data)
    {
        _currentSelectedStage = data;
        // 이름과 설명 전달 없이 창만 띄웁니다.
        _stageView.Show();
    }

    private void HandlePlayGame()
    {
        if (_currentSelectedStage != null)
        {
            GameManager.Instance.StartStage(_currentSelectedStage).Forget();
        }
    }
}