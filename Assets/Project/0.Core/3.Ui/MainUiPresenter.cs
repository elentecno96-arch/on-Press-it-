using Cysharp.Threading.Tasks;
using Project.Core.Managers; 
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
    [SerializeField] private MainUiSoundView _soundView;  // 사운드 전담 뷰

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

        // 2. UI 및 스테이지 해금 상태 갱신 (추가된 부분)
        _isSyncing = true;
        // 오디오 설정 UI 동기화
        SyncUiWithAudio();
       
        // 단순히 자물쇠만 여는 것이 아니라, 전체적인 UI 상태를 동기화하는 관점입니다.
        RefreshAllStageUI();

        _isSyncing = false;

        // 3. BGM 재생 (기존 유지)
        _soundView.PlayMainBgmWithDelay(1.0f).Forget();

        // 난이도 변경 이벤트 구독 (OnEnable에서도 수행하지만 Start 시점 보장)
        _stageView.OnDifficultyDirectionClicked -= HandleDifficultyChange;
        _stageView.OnDifficultyDirectionClicked += HandleDifficultyChange;
    }

    // 슬롯 클릭 시 호출 (배열 데이터를 안전하게 처리)
    public void HandleSlotClicked(StageData[] variants)
    {
        if (variants == null || variants.Length == 0)
        {
            Debug.LogError("[MainUiPresenter] 클릭된 슬롯에 데이터가 없습니다!");
            return;
        }

        Debug.Log($"[MainUiPresenter] 슬롯 클릭 감지: {variants[0].stageName}");

        _soundView.PlaySfxB();
        _activeVariants = variants;

        // 배열 크기에 맞춰 인덱스 초기화 (보통 Normal은 1번 인덱스)
        _currentDifficultyIndex = (variants.Length > 1) ? 1 : 0;

        SyncToGameManager();
        _stageView.Show(); // 이제 비로소 창을 띄웁니다.
    }

    //  난이도 변경 버튼을 눌렀을 때 실행될 실제 로직
    private void HandleDifficultyChange(int direction)
    {
        if (_activeVariants == null) return;

        int nextIndex = _currentDifficultyIndex + direction;

        if (nextIndex >= 0 && nextIndex < _activeVariants.Length)
        {
            _currentDifficultyIndex = nextIndex;
            _soundView.PlaySfxA();

            SyncToGameManager();

            // difficultyType 대신 안전한 stageName이나 인덱스 번호를 출력
            Debug.Log($"[MainUiPresenter] 난이도 변경됨! 현재 선택된 곡: {_currentSelectedStage.stageName} (인덱스: {_currentDifficultyIndex})");
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
        // 중복 구독 방지를 위해 -= 후 += 진행
        _settingView.OnSettingsClick -= OpenSettings;
        _settingView.OnSettingsClick += OpenSettings;
        _settingView.OnSettingsCloseClick -= CloseSettings;
        _settingView.OnSettingsCloseClick += CloseSettings;
        _settingView.OnBgmVolumeChanged -= HandleBgmVolumeChanged;
        _settingView.OnBgmVolumeChanged += HandleBgmVolumeChanged;
        _settingView.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
        _settingView.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        _settingView.OnResetSettingsClick -= HandleResetSettings;
        _settingView.OnResetSettingsClick += HandleResetSettings;

        if (_stageSlots != null)
        {
            foreach (var slot in _stageSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= HandleSlotClicked;
                    slot.OnSlotClicked += HandleSlotClicked;
                }
            }
        }

        _stageView.OnPlayClick -= HandlePlayGame;
        _stageView.OnPlayClick += HandlePlayGame;
        _stageView.OnCloseClick -= HideStageView;
        _stageView.OnCloseClick += HideStageView;
        _stageView.OnDifficultyDirectionClicked -= HandleDifficultyChange;
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