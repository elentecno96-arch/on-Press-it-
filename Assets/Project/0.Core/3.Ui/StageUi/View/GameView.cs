using Cysharp.Threading.Tasks;
using Project.Core.Managers;
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
        [SerializeField] private Button restartConfirmBtn; // 진짜 재시작
        [SerializeField] private Button restartCancelBtn;  // 재시작 취소 (닫기)

        [Header("Exit Popup Buttons")]
        [SerializeField] private Button exitConfirmBtn;    // 진짜 종료
        [SerializeField] private Button exitCancelBtn;     // 종료 취소 (닫기)

        [Header("Setting Panel Buttons")]
        [SerializeField] private Button openRestartPopupBtn;
        [SerializeField] private Button openExitPopupBtn;
        [SerializeField] private Button muteButton;

        private bool _isMuted = false;
        private bool _isActionStarted = false;

        private void Awake()
        {
            // 모든 팝업 초기화 (꺼두기)
            settingPanel?.SetActive(false);
            restartConfirmPopup?.SetActive(false);
            exitConfirmPopup?.SetActive(false);

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
            // 1. 설정창 열기
            settingButton?.onClick.AddListener(() => {
                settingPanel?.SetActive(!settingPanel.activeSelf);
            });

            // 2. 재시작 로직
            openRestartPopupBtn?.onClick.AddListener(() => restartConfirmPopup?.SetActive(true));
            restartCancelBtn?.onClick.AddListener(() => restartConfirmPopup?.SetActive(false)); // 취소: 팝업 닫기

            restartConfirmBtn?.onClick.AddListener(() => {
                if (_isActionStarted) return;
                var currentData = GameManager.Instance.CurrentStageData;
                if (currentData != null)
                {
                    _isActionStarted = true;
                    restartConfirmPopup.SetActive(false);
                    settingPanel.SetActive(false);
                    GameManager.Instance.StartStage(currentData).Forget();
                    _isActionStarted = false;
                }
            });

            // 3. 종료 로직
            openExitPopupBtn?.onClick.AddListener(() => exitConfirmPopup?.SetActive(true));
            exitCancelBtn?.onClick.AddListener(() => exitConfirmPopup?.SetActive(false)); // 취소: 팝업 닫기

            exitConfirmBtn?.onClick.AddListener(() => {
                exitConfirmPopup.SetActive(false);
                settingPanel.SetActive(false);
                LoadingManager.Instance.LoadSceneAsync("Main").Forget();
            });

            // 4. 음소거
            muteButton?.onClick.AddListener(() => {
                _isMuted = !_isMuted;
                AudioListener.volume = _isMuted ? 0f : 1f;
            });
        }

        public void SetStageName(string name) => stageName.text = name;

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