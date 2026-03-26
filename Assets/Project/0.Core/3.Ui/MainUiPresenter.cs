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
        // 1. 모든 매니저가 초기화될 때까지 대기 (가장 중요)
        await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => PlayerManager.Instance != null && PlayerManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => AudioManager.Instance != null && AudioManager.Instance.IsInitialized);

        // 2. UI 초기화 시작
        _isSyncing = true;
        SyncUiWithAudio();

        // --- [추가] 스테이지 해금 상태 업데이트 호출 ---
        UpdateStageUnlockStates();       

        _isSyncing = false;

        _soundView.PlayMainBgmWithDelay(1.0f).Forget();
    }

    // 세이브 데이터를 기반으로 모든 슬롯의 UI를 갱신하는 메서드 ---
    private void UpdateStageUnlockStates()
    {
        if (_stageSlots == null || _stageSlots.Count == 0) return;

        // 첫 번째 스테이지(게임1)는 항상 해금되어 있어야 합니다.
        if (_stageSlots[0] != null)
        {
            _stageSlots[0].SetUnlockState(true);
        }

        // 두 번째 스테이지(게임2)부터는 이전 스테이지 클리어 여부를 체크합니다.
        for (int i = 1; i < _stageSlots.Count; i++)
        {
            if (_stageSlots[i] == null) continue;

            // 해금 규칙: (i-1)번째 스테이지가 클리어되어야 (i)번째 스테이지가 열림
            int previousStageIndex = i; // StageIndex는 1부터 시작한다고 가정 (i=1 -> 게임1의 Index)

            // PlayerManager에게 이전 스테이지 클리어 여부를 물어봅니다.
            bool isPreviousCleared = PlayerManager.Instance.IsStageCleared(previousStageIndex);

            // 해당 슬롯에게 상태 전달
            _stageSlots[i].SetUnlockState(isPreviousCleared);
        }
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
        try
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

        finally
        {
            // 어떤 상황에서도(에러가 나더라도) 다시 소리가 나도록 보장합니다.
            _isSyncing = false;
        }
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