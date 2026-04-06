using Cysharp.Threading.Tasks;
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

        private async void Start()
        {
            await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized);
            await UniTask.WaitUntil(() => PlayerManager.Instance != null && PlayerManager.Instance.IsInitialized);

            InitView(mainView);
            InitView(missionView);
            InitView(helpView);

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
            mainView.SetVisible(type == ProfileTabType.Main);
            missionView.SetVisible(type == ProfileTabType.Mission);
            helpView.SetVisible(type == ProfileTabType.Help);

            switch (type)
            {
                case ProfileTabType.Main:
                    UpdateMainPanel();
                    break;
                case ProfileTabType.Mission:
                    missionView.RefreshAchievementList(); 
                    break;
                case ProfileTabType.Help:
                    break;
            }

            Debug.Log($"[Profile] {type} 탭으로 전환");
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
            ChangeTab(ProfileTabType.Main); 
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnDestroy()
        {
            if (mainView != null)
            {
                mainView.OnNameChanged -= NameChanged;
            }
            mainView.OnTabRequest = null;
            mainView.OnCloseRequest = null;
        }
    }
}
