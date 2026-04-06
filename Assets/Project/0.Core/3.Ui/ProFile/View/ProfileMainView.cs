using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.View
{
    /// <summary>
    /// 프로필 메인 탭 뷰
    /// </summary>
    public class ProfileMainView : BaseProfileView
    {
        [Header("Player Info")]
        [SerializeField] private Image playerIcon;
        [SerializeField] private TMP_InputField nameInputField; // 편집 가능해야 하므로 InputField
        [SerializeField] private Button nameEditBtn;            // 이름 변경 확정 버튼

        [Header("Statistics")]
        [SerializeField] private TextMeshProUGUI achievementCountText;
        [SerializeField] private TextMeshProUGUI totalRankScoreText;

        public Action<string> OnNameChanged; 

        public override void Init()
        {
            base.Init();

            nameEditBtn?.onClick.AddListener(() => {
                OnNameChanged?.Invoke(nameInputField.text);
            });
        }

        public void Setup(Sprite icon, string playerName, int achiCount, int totalScore)
        {
            if (playerIcon != null) playerIcon.sprite = icon;
            if (nameInputField != null) nameInputField.text = playerName;

            achievementCountText.text = achiCount.ToString();
            totalRankScoreText.text = $"{totalScore:N0} PT"; 
        }
    }
}
