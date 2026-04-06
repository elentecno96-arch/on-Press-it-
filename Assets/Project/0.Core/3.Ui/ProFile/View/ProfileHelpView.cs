using Project.UI.Profile.Data;
using Project.UI.Profile.View;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.View
{
    /// <summary>
    /// 프로필 도움말 탭 뷰
    /// </summary>
    public class ProfileHelpView : BaseProfileView
    {
        [SerializeField] private HelpDataSO[] pageDatas;
        [SerializeField] private TextMeshProUGUI[] helpTexts;

        [Header("Help Navigation Buttons")]
        [SerializeField] private List<Button> allPrevButtons;
        [SerializeField] private List<Button> allNextButtons;

        [Header("Pages & Dots")]
        [SerializeField] private GameObject[] pages;
        [SerializeField] private Image[] pageDots;

        [SerializeField] private Color dotOnColor = Color.white;
        [SerializeField] private Color dotOffColor = Color.gray;

        private int _currentPageIndex = 0;

        public override void Init()
        {
            base.Init();

            foreach (var btn in allPrevButtons)
                btn?.onClick.AddListener(() => ChangePage(-1));

            foreach (var btn in allNextButtons)
                btn?.onClick.AddListener(() => ChangePage(1));

            ApplyData();
            UpdateUI();
        }

        private void ChangePage(int direction)
        {
            int nextIndex = _currentPageIndex + direction;
            if (nextIndex < 0 || nextIndex >= pages.Length) return;

            _currentPageIndex = nextIndex;
            UpdateUI();
        }
        private void ApplyData()
        {
            if (pageDatas == null || helpTexts == null) return;

            for (int i = 0; i < helpTexts.Length; i++)
            {
                if (i < pageDatas.Length && pageDatas[i] != null)
                {
                    helpTexts[i].text = pageDatas[i].description;
                }
            }
        }

        private void UpdateUI()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                bool isActive = (i == _currentPageIndex);
                pages[i].SetActive(isActive);

                if (helpTexts != null && i < helpTexts.Length)
                {
                    helpTexts[i].gameObject.SetActive(isActive);
                }

                if (pageDots != null && i < pageDots.Length)
                {
                    pageDots[i].color = isActive ? dotOnColor : dotOffColor;
                }
            }
        }
    }
}
