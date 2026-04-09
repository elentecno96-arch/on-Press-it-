using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Managers;
using Project.Core.Ui.GlobalUi;
using Project.UI.Profile.Data;
using Project.UI.Profile.View;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.Presenter
{
    /// <summary>
    /// 프로필 UI 관리자
    /// </summary>
    public class ProfilePresenter : MonoBehaviour
    {
        [SerializeField] private ProfileMainView mainView;
        [SerializeField] private ProfileMissionView missionView;
        [SerializeField] private ProfileHelpView helpView;

        [SerializeField] private Button openProfileBtn;

        [SerializeField] private Sprite defaultProfileIcon;

        [SerializeField] private RectTransform profilePanel;
        [SerializeField] private CanvasGroup profileCanvasGroup;

        private async void Start()
        {
            await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized);
            await UniTask.WaitUntil(() => PlayerManager.Instance != null && PlayerManager.Instance.IsInitialized);

            InitView(mainView);
            InitView(missionView);
            InitView(helpView);

            if (profilePanel != null) profilePanel.localScale = Vector3.zero;
            if (profileCanvasGroup != null) profileCanvasGroup.alpha = 0f;

            if (openProfileBtn != null)
            {
                openProfileBtn.onClick.AddListener(Show);
            }

            if (mainView != null)
            {
                mainView.OnNameChanged += NameChanged;
            }

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnRankUpdated -= HandleRankUpdate;
                PlayerManager.Instance.OnRankUpdated += HandleRankUpdate;
            }

            gameObject.SetActive(false);

            Debug.Log("[ProfilePresenter] 초기화 및 버튼 연결 완료");
        }

        private void NameChanged(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;

            PlayerManager.Instance.Data.userName = newName;
            PlayerManager.Instance.Save();

            if (GlobalUIPresenter.Instance != null)
                GlobalUIPresenter.Instance.ShowNotification("닉네임이 변경되었습니다.");

            UpdateMainPanel();
        }

        private void HandleRankUpdate(int newRank)
        {
            UpdateMainPanel();
        }

        private void InitView(BaseProfileView view)
        {
            view.Init();
            view.OnTabRequest = ChangeTab;
            view.OnCloseRequest = Hide;
        }

        private void ChangeTab(ProfileTabType type)
        {
            BaseProfileView targetView = type switch
            {
                ProfileTabType.Main => mainView,
                ProfileTabType.Mission => missionView,
                ProfileTabType.Help => helpView,
                _ => null
            };

            mainView.SetVisible(false);
            missionView.SetVisible(false);
            helpView.SetVisible(false);

            if (targetView != null)
            {
                AudioManager.Instance.PlayUISound(UISoundType.Click);

                targetView.SetVisible(true);

                targetView.transform.DOKill();
                targetView.transform.localPosition = new Vector3(0, -20, 0);
                targetView.transform.DOLocalMoveY(0, 0.25f).SetEase(Ease.OutCubic);
            }

            switch (type)
            {
                case ProfileTabType.Main: UpdateMainPanel(); break;
                case ProfileTabType.Mission: missionView.RefreshAchievementList(); break;
            }
        }

        // ProfilePresenter.cs

        private void UpdateMainPanel()
        {
            var pData = PlayerManager.Instance.Data;

            int clearedStages = pData.stageRecords.FindAll(r => r.bestScore > 0).Count;
            int totalStages = 7;

            int unlockedAchi = pData.achievements.FindAll(a => a.isUnlocked).Count;
            int totalAchi = 100;

            string rankStr = pData.currentRank > 0 ? $"{pData.currentRank}" : "조회 중..";
            string rankPercent = pData.currentRank > 0 ? $"{pData.currentPercent:F1}%" : "계산 중..";

            if (mainView != null)
            {
                mainView.Setup(
                    defaultProfileIcon,      // 캐릭터 아이콘
                    pData.userName,          // 유저 닉네임
                    rankStr,                 // 글로벌 랭킹 텍스트
                    rankPercent,             // 상위 % 텍스트
                    clearedStages,           // 현재 클리어 수
                    totalStages,             // 전체 스테이지 수
                    unlockedAchi,            // 현재 업적 달성 수
                    totalAchi                // 전체 업적 수
                );
            }
        }


        public void Show()
        {
            gameObject.SetActive(true);

            AudioManager.Instance.PlayUISound(UISoundType.Open);

            profilePanel.DOKill();
            profileCanvasGroup.DOKill();

            profileCanvasGroup.alpha = 0f;
            profilePanel.localScale = Vector3.one * 0.8f;
            profilePanel.DOScale(1.0f, 0.4f).SetEase(Ease.OutBack);
            profileCanvasGroup.DOFade(1f, 0.3f);

            ChangeTab(ProfileTabType.Main);
        }


        public void Hide()
        {
            AudioManager.Instance.PlayUISound(UISoundType.Cancel);

            profilePanel.DOKill();
            profileCanvasGroup.DOKill();

            profilePanel.DOScale(0.8f, 0.2f).SetEase(Ease.InBack);
            profileCanvasGroup.DOFade(0f, 0.2f).OnComplete(() => {
                gameObject.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnRankUpdated -= HandleRankUpdate;
            }
            if (mainView != null)
            {
                mainView.OnNameChanged -= NameChanged;
            }
            mainView.OnTabRequest = null;
            mainView.OnCloseRequest = null;

            profilePanel.DOKill();
            profileCanvasGroup.DOKill();

        }
    }
}
