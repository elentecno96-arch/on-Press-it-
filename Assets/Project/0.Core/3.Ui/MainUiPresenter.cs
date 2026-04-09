using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using Project.Core.Ui.GlobalUi;
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
    [SerializeField] private StoryImageSequenceUI_DOTween _cutsceneView;

    [Header("--- Stage Slots ---")]
    [SerializeField] private List<StageSlot> _stageSlots;

    private StageData _currentSelectedStage;              // 현재 유저가 클릭한 스테이지 데이터
    private StageData[] _activeVariants;                  // 현재 선택된 스테이지의 난이도 배열
    private int _currentDifficultyIndex = 1;              // 기본 난이도 인덱스 (Normal = 1)
    private const float DefaultVolume = 0.5f;             // 초기화용 기본 볼륨 값

    private bool _isSyncing = false;
    

    private async void Start()
    {
        // 시작하자마자 UI들이 떠 있다면 강제로 끈 상태로 시작
        _settingView.ShowSettings(false);
        _stageView.Hide();

        // 1. 모든 매니저가 초기화될 때까지 대기 (가장 중요)
        await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => PlayerManager.Instance != null && PlayerManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => AudioManager.Instance != null && AudioManager.Instance.IsInitialized);

        _isSyncing = true;
        SyncUiWithAudio(); // 여기서 슬라이더 값을 세팅할 때 UI가 켜지는지 확인 필요
        RefreshAllStageUI();
        _isSyncing = false;

        // 서버(Firebase)로부터 실제 유저 데이터를 완전히 받아올 때까지 대기
        // 이 과정이 끝나야 stageRecords에 실제 점수들이 채워집니다.
        Debug.Log("[MainUiPresenter] 최신 플레이어 데이터를 서버와 동기화합니다...");
        await PlayerManager.Instance.SyncWithServer();

        TryPlayIntroCutscene();

        // UI 및 스테이지 해금 상태 갱신 (추가된 부분)
        _isSyncing = true;
        // 오디오 설정 UI 동기화
        SyncUiWithSettings();
        // 단순히 자물쇠만 여는 것이 아니라, 전체적인 UI 상태를 동기화하는 관점입니다.
        RefreshAllStageUI();

        _isSyncing = false;

        AudioManager.Instance.PlayBGM(AudioManager.Instance.MainMenuBgmClip);

        // 난이도 변경 이벤트 구독 (OnEnable에서도 수행하지만 Start 시점 보장)
        _stageView.OnDifficultyDirectionClicked -= HandleDifficultyChange;
        _stageView.OnDifficultyDirectionClicked += HandleDifficultyChange;
    }

    // 슬롯 클릭 시 호출 (배열 데이터를 안전하게 처리)
    public void HandleSlotClicked(StageData[] variants)
    {
        if (variants == null || variants.Length == 0) return;

        AudioManager.Instance.PlayUISound(UISoundType.Open);
        _activeVariants = variants;

        _currentDifficultyIndex = 0;

        SyncToGameManager();

        _stageView.UpdateStageDetails(_activeVariants[_currentDifficultyIndex]);
        _stageView.Show();
    }

    //  난이도 변경 버튼을 눌렀을 때 실행될 실제 로직
    private void HandleDifficultyChange(int direction)
    {
        if (_activeVariants == null) return;

        int nextIndex = _currentDifficultyIndex + direction;

        if (nextIndex >= 0 && nextIndex < _activeVariants.Length)
        {
            StageData targetData = _activeVariants[nextIndex];

            if (targetData.difficulty == StageData.Difficulty.Hard)
            {
                if (PlayerManager.Instance != null && !PlayerManager.Instance.IsStageCleared(4))
                {
                    AudioManager.Instance.PlayUISound(UISoundType.Error);
                    Debug.LogWarning("<color=yellow>[MainUI]</color> 보스 스테이지(4번)를 클리어해야 하드 모드가 해금됩니다!");

                    if (GlobalUIPresenter.Instance != null)
                        GlobalUIPresenter.Instance.ShowNotification("보스 스테이지를 먼저 클리어하세요!");

                    return;
                }
            }

            // 해금되었거나 노말 난이도인 경우 변경 확정
            _currentDifficultyIndex = nextIndex;
            AudioManager.Instance.PlayUISound(UISoundType.Click);

            SyncToGameManager();
            _stageView.UpdateStageDetails(_currentSelectedStage);
        }
    }

    //  현재 선택된 데이터를 확정하고 GameManager에 전달
    private void SyncToGameManager()
    {
        if (_activeVariants != null && _activeVariants.Length > _currentDifficultyIndex)
        {
            _currentSelectedStage = _activeVariants[_currentDifficultyIndex];

            // 이미지 흐름도대로 GameManager에 보관
            GameManager.Instance.SetCurrentStage(_currentSelectedStage);
        }
    }

    //  외부에서도 호출 가능하도록 퍼블릭으로 선언된 새로고침 메서드
    public void RefreshAllStageUI()
    {
        // 기존에 작성했던 자물쇠 해금 로직을 실행합니다.
        UpdateStageUnlockStates();

        // 여기서 점수 텍스트 갱신 등의 로직을 함께 넣을 수 있습니다.
        Debug.Log("[MainUiPresenter] 모든 스테이지 UI 상태가 갱신되었습니다.");
    }

    // 세이브 데이터를 기반으로 모든 슬롯의 UI를 갱신하는 메서드 ---
    private void UpdateStageUnlockStates()
    {
        // 1. 리스트가 비어있는지 먼저 확인 (방어 코드)
        if (_stageSlots == null || _stageSlots.Count == 0)
        {
            Debug.LogWarning("[MainUiPresenter] _stageSlots 리스트가 비어있습니다. 인스펙터를 확인하세요.");
            return;
        }

        // 2. foreach문을 통해 리스트 안의 각 'slot'을 하나씩 검사
        foreach (StageSlot slot in _stageSlots) // 여기서 StageSlot은 클래스 이름입니다.
        {
            if (slot == null) continue; // 슬롯이 비어있으면 건너뜀

            // 3. 해당 슬롯의 스테이지 번호(1, 2, 3...)를 가져옴
            int myStageNum = slot.GetStageIndex();
            bool isUnlocked = false;

            // 4. 해금 판정 로직
            if (myStageNum == 1)
            {
                // 1번 스테이지는 무조건 오픈
                isUnlocked = true;
            }
            else if (myStageNum > 1)
            {
                // 이전 스테이지(내 번호 - 1)가 클리어 되었는지 확인              
                isUnlocked = PlayerManager.Instance.IsStageCleared(myStageNum - 1);
            }

            // 5. 실제 UI(자물쇠 등)에 상태 반영
            slot.SetUnlockState(isUnlocked);

            // 6. 로그로 확인
            Debug.Log($"[검사] 스테이지 {myStageNum}번 슬롯 | 이전(제{myStageNum - 1}번) 클리어여부: {isUnlocked}");
        }
    }

    private void OnEnable()
    {
        _settingView.OnSettingsClick += OpenSettings;
        _settingView.OnSettingsCloseClick += CloseSettings;
        _settingView.OnBgmVolumeChanged += HandleBgmVolumeChanged;
        _settingView.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        _settingView.OnResetSettingsClick += HandleResetSettings;
        _settingView.OnVibrationChanged += HandleVibrationChanged;

        if (_stageSlots != null)
        {
            foreach (var slot in _stageSlots)
            {
                if (slot != null) slot.OnSlotClicked += HandleSlotClicked;
            }
        }

        _stageView.OnPlayClick += HandlePlayGame;
        _stageView.OnCloseClick += HideStageView;
        _stageView.OnDifficultyDirectionClicked += HandleDifficultyChange;
    }

    private void OnDisable()
    {
        if (_settingView != null)
        {
            _settingView.OnSettingsClick -= OpenSettings;
            _settingView.OnSettingsCloseClick -= CloseSettings;
            _settingView.OnBgmVolumeChanged -= HandleBgmVolumeChanged;
            _settingView.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
            _settingView.OnResetSettingsClick -= HandleResetSettings;
            _settingView.OnVibrationChanged -= HandleVibrationChanged;
        }

        if (_stageSlots != null)
        {
            foreach (var slot in _stageSlots)
            {
                if (slot != null) slot.OnSlotClicked -= HandleSlotClicked;
            }
        }

        if (_stageView != null)
        {
            _stageView.OnPlayClick -= HandlePlayGame;
            _stageView.OnCloseClick -= HideStageView;
            _stageView.OnDifficultyDirectionClicked -= HandleDifficultyChange;
        }
    }

    // OnDisable은 기존과 동일하게 유지 (이벤트 해제)

    private void SyncUiWithAudio()
    {
        if (_settingView == null || PlayerManager.Instance == null) return;

        _settingView.SetSettingValues(
            AudioManager.Instance.BgmVolume,
            AudioManager.Instance.SfxVolume,
            PlayerManager.Instance.Data.isVibrationOn
        );
    }

    private void HandleResetSettings()
    {
        try
        {
            _isSyncing = true;

            AudioManager.Instance.SetVolume("BGM", DefaultVolume);
            AudioManager.Instance.SetVolume("SFX", DefaultVolume);

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.Data.isVibrationOn = true;
                PlayerManager.Instance.Save();
            }

            _settingView.SetSettingValues(DefaultVolume, DefaultVolume, true);
        }
        finally { _isSyncing = false; }
    }

    private void SyncUiWithSettings()
    {
        if (_settingView == null || PlayerManager.Instance == null) return;

        _isSyncing = true;
        _settingView.SetSettingValues(
            AudioManager.Instance.BgmVolume,
            AudioManager.Instance.SfxVolume,
            PlayerManager.Instance.Data.isVibrationOn
        );
        _isSyncing = false;
    }

    private void HandleVibrationChanged(bool isOn)
    {
        if (PlayerManager.Instance == null || _isSyncing) return;

        // 데이터 저장
        PlayerManager.Instance.Data.isVibrationOn = isOn;
        PlayerManager.Instance.Save();

        Debug.Log($"[MainUiPresenter] 진동 설정 변경 및 저장됨: {isOn}");
    }

    private void TryPlayIntroCutscene()
    {
        var player = PlayerManager.Instance;

        if (!player.Data.hasSeenIntroCutscene)
        {
            Debug.Log("[Cutscene] 신규 유저 → 인트로 컷씬 재생");

            _cutsceneView.gameObject.SetActive(true);
            _cutsceneView.RestartSequence();

            player.Data.hasSeenIntroCutscene = true;
            player.Save();
        }
    }

    public void PlayCutsceneAgain()
    {
        _cutsceneView.gameObject.SetActive(true);
        _cutsceneView.RestartSequence();
    }

    public void HandleBgmVolumeChanged(float vol)
    {
        if (_isSyncing) return;
        AudioManager.Instance.SetVolume("BGM", vol);
    }

    public void HandleSfxVolumeChanged(float vol)
    {
        if (_isSyncing) return;
        AudioManager.Instance.SetVolume("SFX", vol);
    }

    private void OpenSettings()
    {
        SyncUiWithSettings();

        _settingView.ShowSettings(true);
    }

    private void CloseSettings()
    {
        _settingView.ShowSettings(false);
        AudioManager.Instance.AudioSaveSettings();
    }

    private void HideStageView()
    {
        _stageView.Hide();
    }

    private void HandlePlayGame()
    {
        if (_currentSelectedStage != null)
        {
            AudioManager.Instance.PlayUISound(UISoundType.Check);
            AudioManager.Instance.StopBGM();
            GameManager.Instance.StartStage(_currentSelectedStage).Forget();
        }
    }
}