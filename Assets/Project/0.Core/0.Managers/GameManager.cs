using Cysharp.Threading.Tasks;
using Project.Core.Ui.GlobalUi;
using Project.Core.Utilities;
using Project.Rhythm.Data;
using UnityEngine;

namespace Project.Core.Managers
{
    /// <summary>
    /// 이 친구는 게임의 흐름 담당할거임
    /// </summary>
    public class GameManager : BaseSingleton<GameManager>
    {
        public StageData CurrentStageData { get; private set; }
        // Presenter가 데이터를 넘겨줄 때 사용할 메서드
        public void SetCurrentStage(StageData data)
        {
            CurrentStageData = data;
            Debug.Log($"[GameManager] 현재 스테이지가 설정됨: {data.stageName}");
        }
        public override async UniTask Initialize()
        {
            Debug.Log("모든 매니저 초기화 진행");

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            await UniTask.Yield();

            if (IsInitialized) return;

            if (GlobalUIPresenter.Instance != null)
            {
                await GlobalUIPresenter.Instance.Initialize();
            }

            await PlayerManager.Instance.Initialize();
            await FirebaseManager.Instance.Initialize();

            if (FirebaseManager.Instance.IsInitialized)
            {
                await PlayerManager.Instance.SyncWithServer();
            }

            if (InputManager.Instance != null)
            {
                await InputManager.Instance.Initialize();
            }

            await AudioManager.Instance.Initialize();
            await AchievementManager.Instance.Initialize();

            Debug.Log("모든 매니저 초기화 진행 완료");
            IsInitialized = true;
        }

        /// <summary>
        /// 외부에서 스테이지를 통해 게임씬에 이동하려고 할 때 호출
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async UniTaskVoid StartStage(StageData data)
        {
            CurrentStageData = data;
            await LoadingManager.Instance.LoadStageAsync("Game");
        }

        public async UniTask EnterGameScene(string sceneName)
        {
            await LoadingManager.Instance.LoadSceneAsync(sceneName);
        }
    }
}
