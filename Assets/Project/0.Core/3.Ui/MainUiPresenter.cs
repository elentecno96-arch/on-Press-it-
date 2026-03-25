using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 UI의 중재자 역할을 하는 Presenter 클래스입니다.
/// View(UI)의 이벤트를 듣고 Manager(데이터/로직)를 조작합니다.
/// </summary>
public class MainUiPresenter : MonoBehaviour
{
    [Header("--- Views ---")]
    [SerializeField] private SettingUIView _settingView;            // 설정창 UI
    [SerializeField] private StageUiView _stageView;                // 스테이지 정보창 UI

    [Header("--- Stage Slots ---")]
    [SerializeField] private List<StageSlot> _stageSlots;           // 스테이지 선택 버튼 리스트
    [SerializeField] private AudioClip _mainBgmClip;                // 메인 화면 배경음악

    private StageData _currentSelectedStage;                        // 현재 유저가 클릭한 스테이지 데이터
    private const float DefaultVolume = 0.5f;                       // 초기화용 기본 볼륨 값

    private void Start()
    {
        // [초기화] 씬이 시작될 때 현재 저장된 오디오 설정을 UI에 동기화
        SyncUiWithAudio();

        // 2. 0.5초 대기 후 배경음 재생 (비동기 실행)
        PlayMainBgmWithDelay(1.0f).Forget();
    }

    /// 지정된 시간만큼 대기한 후 메인 배경음악을 재생합니다.
    private async UniTaskVoid PlayMainBgmWithDelay(float delaySeconds)
    {
        // 1.0초(1000ms) 동안 대기
        await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));

        // 대기 후 AudioManager를 통해 재생
        if (_mainBgmClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(_mainBgmClip);
        }
    }

    private void OnEnable()
    {
        // --- SettingView 이벤트 구독 ---
        // 람다 식 대신 메서드 참조(OpenSettings 등)를 사용하여 OnDisable에서 안전하게 해제 가능하게 함
        _settingView.OnSettingsClick += OpenSettings;
        _settingView.OnSettingsCloseClick += CloseSettings;

        _settingView.OnBgmVolumeChanged += HandleBgmVolumeChanged;
        _settingView.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        _settingView.OnResetSettingsClick += HandleResetSettings;

        // UI가 활성화될 때마다 오디오 상태를 최신으로 갱신
        SyncUiWithAudio();

        // --- Stage Slots 이벤트 구독 ---
        foreach (var slot in _stageSlots)
        {
            if (slot != null) slot.OnSlotClicked += HandleStageSelected;
        }

        // --- StageView(팝업) 이벤트 구독 ---
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

    //#endregion

    //#region 오디오 로직 (Audio Logic)

    /// <summary>
    /// AudioManager의 실제 볼륨 값을 UI 슬라이더 위치에 반영합니다.
    /// </summary>
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

    // 슬라이더 조절 시 AudioManager를 통해 즉시 볼륨 변경
    private void HandleBgmVolumeChanged(float vol) => AudioManager.Instance?.SetVolume("BGM", vol);
    private void HandleSfxVolumeChanged(float vol) => AudioManager.Instance?.SetVolume("SFX", vol);

    /// 볼륨을 기본값(0.5)으로 되돌리고 UI에 즉시 반영합니다.
    private void HandleResetSettings()
    {
        AudioManager.Instance.SetVolume("BGM", DefaultVolume);
        AudioManager.Instance.SetVolume("SFX", DefaultVolume);
        _settingView.SetSliderValues(DefaultVolume, DefaultVolume);
    }

    //region 뷰 제어 (View Control)
    private void OpenSettings() => _settingView.ShowSettings(true);
    private void CloseSettings()
    {
        _settingView.ShowSettings(false);
        AudioManager.Instance?.AudioSaveSettings(); // 닫을 때 저장 로직 포함
    }

    private void HideStageView() => _stageView.Hide();

    /// 특정 스테이지 슬롯을 클릭했을 때 호출됩니다.
    private void HandleStageSelected(StageData data)
    {
        _currentSelectedStage = data;   // 선택된 데이터 보관      
        _stageView.Show();              // 스테이지 정보 팝업 표시
    }

    /// 스테이지 정보창에서 '시작' 버튼을 눌렀을 때 게임을 실행합니다.
    private void HandlePlayGame()
    {
        if (_currentSelectedStage != null)
        {
            AudioManager.Instance.StopBGM();
            // GameManager를 통해 스테이지 시작 (비동기 Task 실행)
            GameManager.Instance.StartStage(_currentSelectedStage).Forget();
        }
    }
}