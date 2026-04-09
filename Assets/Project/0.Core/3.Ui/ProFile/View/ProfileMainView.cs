using Project.Core.Managers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.View
{
    public class ProfileMainView : BaseProfileView
    {
        [Header("--- Player Info ---")]
        [SerializeField] private Image playerIcon;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button nameEditBtn;                            // 연필 아이콘 버튼
        [SerializeField] private TextMeshProUGUI editBtnText;                   // 버튼 텍스트 (변경/확인 전환용)

        [Header("--- Statistics (Dashboard) ---")]
        [SerializeField] private TextMeshProUGUI globalRankText;                // 글로벌 랭킹 (예: 1위)
        [SerializeField] private TextMeshProUGUI rankPercentText;               // 상위 % (예: 0.1%)
        [SerializeField] private TextMeshProUGUI stageClearCountText;           // 스테이지 클리어 (예: 2/28)
        [SerializeField] private TextMeshProUGUI stagePercentText;              // 달성률 %
        [SerializeField] private TextMeshProUGUI achievementProgressText;       // 업적 달성 (예: 25/100)
        [SerializeField] private Slider achievementSlider;                      // 업적 게이지

        public Action<string> OnNameChanged;
        public Action OnReplayIntroClicked;                                     // 프레젠터로 전달할 이벤트

        private bool _isEditMode = false;

        public override void Init()
        {
            base.Init();

            nameEditBtn?.onClick.AddListener(HandleNameEdit);

            if (nameInputField != null) nameInputField.interactable = false;
        }

        private void HandleNameEdit()
        {
            if (!_isEditMode)
            {
                AudioManager.Instance.PlayUISound(UISoundType.Click);

                _isEditMode = true;
                nameInputField.interactable = true;
                nameInputField.ActivateInputField(); 
                if (editBtnText != null) editBtnText.text = "확인";
            }
            else
            {
                AudioManager.Instance.PlayUISound(UISoundType.Check);

                _isEditMode = false;
                nameInputField.interactable = false;
                if (editBtnText != null) editBtnText.text = "변경";

                OnNameChanged?.Invoke(nameInputField.text);
            }
        }

        public void Setup(Sprite icon, string playerName, string rank, string rankPercent,
                          int clearedStage, int totalStage, int achiCount, int totalAchi)
        {
            if (playerIcon != null) playerIcon.sprite = icon;
            if (nameInputField != null) nameInputField.text = playerName;

            globalRankText.text = rank;
            rankPercentText.text = $" {rankPercent}";

            stageClearCountText.text = $"{clearedStage}/{totalStage}";
            float stageRate = (float)clearedStage / totalStage * 100f;
            stagePercentText.text = $"{stageRate:F0}%";

            achievementProgressText.text = $"{achiCount}/{totalAchi}";
            if (achievementSlider != null)
                achievementSlider.value = (float)achiCount / totalAchi;
        }
    }
}