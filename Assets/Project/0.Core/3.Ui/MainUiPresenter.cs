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

    private StageData _currentSelectedStage;
    private const float DefaultVolume = 0.5f;

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
            _stageView.OnPlayClick -= HandlePlayGame;
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