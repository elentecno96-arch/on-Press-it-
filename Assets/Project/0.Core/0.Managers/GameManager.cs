using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using UnityEngine;

namespace Project.Core.Managers
{
    /// <summary>
    /// 이 친구는 게임의 흐름 담당할거임
    /// </summary>
    public class GameManager : BaseSingleton<GameManager>
    {
        public override async UniTask Initialize()
        {
            await UniTask.Yield();
            if (IsInitialized) return;

            //전역 매니저 기본 초기화
            // await AudioManager.Instance.Initialize();
            // await PlayerManager.Instance.Initialize();
            // await StageManager.Instance.Initialize();

            IsInitialized = true;
        }

        public async UniTask EnterGameScene(string sceneName)
        {
            await LoadingManager.Instance.LoadSceneAsync(sceneName, InitGameScene());
        }

        private async UniTask InitGameScene()
        {
            await UniTask.Yield();

            Debug.Log("[GameManager] InitGameScene 완료");
        }
    }
}
