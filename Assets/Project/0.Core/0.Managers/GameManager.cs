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
        /// 메인에서 스테이지를 클릭했을 때 호출
        /// </summary>
        public void SelectStage(StageData stageData)
        {
            CurrentStageData = stageData;
        }

        public async UniTask EnterGameScene(string sceneName)
        {
            await LoadingManager.Instance.LoadSceneAsync(sceneName);
        }
    }
}
