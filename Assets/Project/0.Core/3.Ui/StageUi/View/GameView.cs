using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.Ui.StageUi.View
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI stageName;

        [Header("Progress UI Settings")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Main Panels")]
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Button settingButton;

        [Header("Confirmation Popups")]
        [SerializeField] private GameObject restartConfirmPopup;
        [SerializeField] private GameObject exitConfirmPopup;

        [Header("Restart Popup Buttons")]
        [SerializeField] private Button restartConfirmBtn;
        [SerializeField] private Button restartCancelBtn;

        [Header("Exit Popup Buttons")]
        [SerializeField] private Button exitConfirmBtn;
        [SerializeField] private Button exitCancelBtn;

        [Header("Setting Panel Buttons")]
        [SerializeField] private Button openRestartPopupBtn;
        [SerializeField] private Button openExitPopupBtn;
        [SerializeField] private Button muteButton;
        [SerializeField] private Button vibrationButton;

        private bool _isMuted = false;
        private bool _isActionStarted = false;

        // 자동 숨김용 토큰
        private CancellationTokenSource _hideTokenSource;

        private void Awake()
        {
            if (settingPanel != null) settingPanel.SetActive(false);
            if (restartConfirmPopup != null) restartConfirmPopup.SetActive(false);
            if (exitConfirmPopup != null) exitConfirmPopup.SetActive(false);

            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
                progressSlider.value = 0f;
                progressSlider.interactable = false;
            }

            UpdateProgress(0f);
            InitButtonEvents();
        }

        private void InitButtonEvents()
        {
            if (settingButton != null)
            {
                settingButton.onClick.AddListener(() => {
                    if (settingPanel == null) return;

                    bool nextState = !settingPanel.activeSelf;
                    settingPanel.SetActive(nextState);

                    if (nextState) StartAutoHideTimer().Forget();
                    else CancelTimer();
                });
            }

            if (openRestartPopupBtn != null)
            {
                openRestartPopupBtn.onClick.AddListener(() => {
                    if (restartConfirmPopup != null) restartConfirmPopup.SetActive(true);
                    RefreshTimer();
                });
            }

            if (restartCancelBtn != null)
            {
                restartCancelBtn.onClick.AddListener(() => {
                    if (restartConfirmPopup != null) restartConfirmPopup.SetActive(false);
                    RefreshTimer();
                });
            }

            if (restartConfirmBtn != null)
            {
                restartConfirmBtn.onClick.AddListener(() => {
                    if (_isActionStarted) return;

                    var currentData = GameManager.Instance.CurrentStageData;
                    if (currentData != null)
                    {
                        _isActionStarted = true;
                        CancelTimer();
                        if (restartConfirmPopup != null) restartConfirmPopup.SetActive(false);
                        if (settingPanel != null) settingPanel.SetActive(false);
                        GameManager.Instance.StartStage(currentData).Forget();
                        _isActionStarted = false;
                    }
                });
            }

            if (openExitPopupBtn != null)
            {
                openExitPopupBtn.onClick.AddListener(() => {
                    if (exitConfirmPopup != null) exitConfirmPopup.SetActive(true);
                    RefreshTimer();
                });
            }

            if (exitCancelBtn != null)
            {
                exitCancelBtn.onClick.AddListener(() => {
                    if (exitConfirmPopup != null) exitConfirmPopup.SetActive(false);
                    RefreshTimer();
                });
            }

            if (exitConfirmBtn != null)
            {
                exitConfirmBtn.onClick.AddListener(() => {
                    CancelTimer();
                    if (exitConfirmPopup != null) exitConfirmPopup.SetActive(false);
                    if (settingPanel != null) settingPanel.SetActive(false);
                    LoadingManager.Instance.LoadSceneAsync("Main").Forget();
                });
            }

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(() => {
                    _isMuted = !_isMuted;
                    AudioListener.volume = _isMuted ? 0f : 1f;
                    RefreshTimer();
                });
            }

            if (vibrationButton != null)
            {
                vibrationButton.onClick.AddListener(() => {
                    if (PlayerManager.Instance != null && PlayerManager.Instance.Data != null)
                    {
                        bool currentVib = PlayerManager.Instance.Data.isVibrationOn;
                        PlayerManager.Instance.Data.isVibrationOn = !currentVib;

                        PlayerManager.Instance.Save();

                        if (PlayerManager.Instance.Data.isVibrationOn)
                        {
                            #if UNITY_ANDROID || UNITY_IOS
                            Handheld.Vibrate();
                            #endif
                        }

                        Debug.Log($"[Option] 진동 설정 변경: {PlayerManager.Instance.Data.isVibrationOn}");
                    }
                    RefreshTimer();
                });
            }
        }

        private async UniTaskVoid StartAutoHideTimer()
        {
            CancelTimer();
            _hideTokenSource = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(2000, cancellationToken: _hideTokenSource.Token);

                if (restartConfirmPopup == null || exitConfirmPopup == null) return;

                if (restartConfirmPopup.activeSelf || exitConfirmPopup.activeSelf)
                {
                    return;
                }

                if (settingPanel != null) settingPanel.SetActive(false);
            }
            catch (System.OperationCanceledException)
            {
                // 취소 시 예외 처리
            }
        }

        private void RefreshTimer()
        {
            if (settingPanel != null && settingPanel.activeSelf)
            {
                StartAutoHideTimer().Forget();
            }
        }

        private void CancelTimer()
        {
            if (_hideTokenSource != null)
            {
                _hideTokenSource.Cancel();
                _hideTokenSource.Dispose();
                _hideTokenSource = null;
            }
        }

        private void OnDestroy()
        {
            CancelTimer();
        }

        public void SetStageName(string name)
        {
            if (stageName != null) stageName.text = name;
        }

        public void UpdateProgress(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            if (progressSlider != null) progressSlider.value = clampedProgress;

            if (progressText != null)
            {
                int percent = Mathf.FloorToInt(clampedProgress * 100f);
                progressText.text = $"{percent}%";
            }
        }
    }
}