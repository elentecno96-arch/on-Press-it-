using Cysharp.Threading.Tasks;
using Project.Core.Managers; // 오직 AudioManager.Instance null 체크와 GameManager를 위해서만 유지
using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 UI의 중재자 역할을 하는 Presenter 클래스입니다.
/// View(UI)의 이벤트를 듣고 Manager(데이터/로직)를 조작합니다.
/// </summary>
public class MainUiPresenter : MonoBehaviour
{
    [Header("--- Views ---")]
    [SerializeField] private SettingUIView _settingView;
    [SerializeField] private StageUiView _stageView;
    [SerializeField] private MainUiSoundView _soundView; // 사운드 전담 뷰

    [Header("--- Stage Slots ---")]
    [SerializeField] private List<StageSlot> _stageSlots;

    private StageData _currentSelectedStage;                        // 현재 유저가 클릭한 스테이지 데이터
    private const float DefaultVolume = 0.5f;                       // 초기화용 기본 볼륨 값

    private bool _isSyncing = false;
    private async void Start()
    {
        await UniTask.WaitUntil(() => AudioManager.Instance != null);

        // 1. 초기화 시작 (소리 차단)
        _isSyncing = true;
        SyncUiWithAudio();
        _isSyncing = false; // 2. 초기화 완료 (차단 해제)

        _soundView.PlayMainBgmWithDelay(1.0f).Forget();
    }

    private void OnEnable()
    {
        _settingView.OnSettingsClick += OpenSettings;
        _settingView.OnSettingsCloseClick += CloseSettings;
        _settingView.OnBgmVolumeChanged += HandleBgmVolumeChanged;
        _settingView.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        _settingView.OnResetSettingsClick += HandleResetSettings;

        foreach (var slot in _stageSlots)
            if (slot != null) slot.OnSlotClicked += HandleStageSelected;

        _stageView.OnPlayClick += HandlePlayGame;
        _stageView.OnCloseClick += HideStageView;
    }    

    private void OnDisable()
    {
        // [메모리 누수 방지] 오브젝트가 비활성화될 때 모든 이벤트를 해제 (중복 구독 방지)
        if (_settingView != null)
        {
            _settingView.OnSettingsClick -= OpenSettings;
            _settingView.OnSettingsCloseClick -= CloseSettings;
            _settingView.OnBgmVolumeChanged -= HandleBgmVolumeChanged;
            _settingView.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
            _settingView.OnResetSettingsClick -= HandleResetSettings;
        }

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
            _stageView.OnCloseClick -= HideStageView;
        }
    }   

    // OnDisable은 기존과 동일하게 유지 (이벤트 해제)

    private void SyncUiWithAudio()
    {
        // _soundView나 _settingView가 연결되지 않았을 경우를 대비한 방어 코드
        if (_soundView == null || _settingView == null)
        {
            Debug.LogWarning("MainUiPresenter: _soundView 또는 _settingView가 연결되지 않았습니다!");
            return;
        }

        // 두 뷰가 모두 존재할 때만 실행
        _settingView.SetSliderValues(_soundView.BgmVolume, _soundView.SfxVolume);
    }        

    private void HandleResetSettings()
    {
        // 1. 리셋 과정 시작 (이벤트에 의한 효과음 차단)
        _isSyncing = true;

        _soundView.PlaySfxC(); // 리셋 버튼 클릭 자체의 소리 (필요하다면 유지)
        _soundView.SetVolume("BGM", DefaultVolume);
        _soundView.SetVolume("SFX", DefaultVolume);

        // 이 함수 호출로 인해 HandleSfxVolumeChanged가 실행되지만, 
        // _isSyncing이 true라 소리는 나지 않습니다.
        _settingView.SetSliderValues(DefaultVolume, DefaultVolume);


    }
    public void HandleBgmVolumeChanged(float vol)
    {
        _soundView.SetVolume("BGM", vol);
    }

    public void HandleSfxVolumeChanged(float vol)
    {
        _soundView.SetVolume("SFX", vol);

        // [중요] 동기화나 리셋 중이 아닐 때(사용자가 직접 조작할 때)만 효과음 재생
        if (!_isSyncing)
        {
            _soundView.PlaySfxC();
        }
    }

    private void OpenSettings()
    {
        _soundView.PlaySfxA();
        _settingView.ShowSettings(true);
    }

    private void CloseSettings()
    {
        _soundView.PlaySfxC();
        _settingView.ShowSettings(false);
        _soundView.SaveAudioSettings(); // 저장 명령 위임
    }

    private void HideStageView()
    {
        _soundView.PlaySfxC();
        _stageView.Hide();
    }

    private void HandleStageSelected(StageData data)
    {
        _soundView.PlaySfxB();
        _currentSelectedStage = data;
        _stageView.Show();
    }

    private void HandlePlayGame()
    {
        if (_currentSelectedStage != null)
        {
            _soundView.PlaySfxC();
            _soundView.StopBgm();
            GameManager.Instance.StartStage(_currentSelectedStage).Forget();
        }
    }
}