using Cysharp.Threading.Tasks;
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
        public override async UniTask Initialize()
        {
            Debug.Log("모든 매니저 초기화 진행");

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            await UniTask.Yield();

            if (IsInitialized) return;

            if (InputManager.Instance != null)
            {
                await InputManager.Instance.Initialize();
            }

            //전역 매니저 기본 초기화
            await AudioManager.Instance.Initialize();
            await PlayerManager.Instance.Initialize();

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
