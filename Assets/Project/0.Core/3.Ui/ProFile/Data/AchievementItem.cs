using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.View
{
    /// <summary>
    /// 프로필 업적 탭 아이템 뷰
    /// </summary>
    public class AchievementItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;    // 업적 이름
        [SerializeField] private TextMeshProUGUI dateText;     // 달성 날짜
        [SerializeField] private Slider progressSlider;        // 게이지 (달성했으므로 1로 고정)
        [SerializeField] private Image iconImage;             // 아이콘 (필요시 회색조 해제용)

        public void Setup(string title, string date)
        {
            if (titleText != null) titleText.text = title;
            if (dateText != null) dateText.text = date;

            if (progressSlider != null) progressSlider.value = 1f;
            if (iconImage != null) iconImage.color = Color.white;
        }
    }
}
