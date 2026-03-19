using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.Ui.StageUi.View
{
    /// <summary>
    /// Game씬의 InGame UI 담당 View
    /// Image의 FillAmount를 이용해 진행도를 표시합니다.
    /// </summary>
    public class GameView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI stageName;

        [SerializeField] private GameObject progressBar;
        [SerializeField] private Image progressBack;  // 배경 이미지 (필요시 활성/비활성용)
        [SerializeField] private Image progressGauge; // 실제 차오르는 이미지 (Image Type: Filled 필수)

        [SerializeField] private Button settingButton;
        [SerializeField] private GameObject settingPanel;

        [SerializeField] private Button exitToMainButton;

        private void Awake()
        {
            if (settingPanel != null) settingPanel.SetActive(false);
            if (settingButton != null)
            {
                settingButton.onClick.AddListener(() => {
                    if (settingPanel != null)
                    {
                        bool isActive = settingPanel.activeSelf;
                        settingPanel.SetActive(!isActive);
                        Debug.Log($"<color=white>[GameView]</color> 설정창 {(!isActive ? "활성화" : "비활성화")}");
                    }
                });
            }

            if (exitToMainButton != null)
            {
                exitToMainButton.onClick.AddListener(() => {
                    LoadingManager.Instance.LoadSceneAsync("Main").Forget();
                    Debug.Log("<color=orange>[GameView]</color> 게임 중단 및 메인 씬 이동 요청");
                });
            }

            UpdateProgress(0f);
        }

        public void SetStageName(string name) => stageName.text = name;

        /// <summary>
        /// 0.0 ~ 1.0 사이의 값을 받아 이미지의 FillAmount를 조절
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (progressGauge != null)
            {
                progressGauge.fillAmount = Mathf.Clamp01(progress);
            }
        }

        public void OpenSetting() => settingPanel?.SetActive(true);
        public void CloseSetting() => settingPanel?.SetActive(false);
    }
}