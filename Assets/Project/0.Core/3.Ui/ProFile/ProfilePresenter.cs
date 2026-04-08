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

        private void UpdateMainPanel()
        {
            var playerData = PlayerManager.Instance.Data;

            int clearedCount = playerData.achievements.FindAll(a => a.isUnlocked).Count;

            int totalScore = 0;
            foreach (var record in playerData.stageRecords)
            {
                totalScore += (int)record.bestScore;
            }

            mainView.Setup(
                defaultProfileIcon,
                playerData.userName,
                clearedCount,
                totalScore
            );
        }
        public void Show()
        {
            gameObject.SetActive(true);

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
            profilePanel.DOKill();
            profileCanvasGroup.DOKill();

            profilePanel.DOScale(0.8f, 0.2f).SetEase(Ease.InBack);
            profileCanvasGroup.DOFade(0f, 0.2f).OnComplete(() => {
                gameObject.SetActive(false);
            });
        }

        private void OnDestroy()
        {
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
