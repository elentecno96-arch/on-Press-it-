using Cysharp.Threading.Tasks;
using Project.Core.Utilities;

namespace Project.Core.Managers
{
    /// <summary>
    /// 이 친구는 게임의 흐름 담당할거임
    /// </summary>
    public class GameManager : BaseSingleton<GameManager>
    {
        public override async UniTask Initialize()
        {
            if (IsInitialized) return;

            //전역 매니저 기본 초기화
            // await AudioManager.Instance.Initialize();
            // await PlayerManager.Instance.Initialize();
            // await StageManager.Instance.Initialize();

            IsInitialized = true;
        }

        public async UniTask EnterGameScene(string sceneName)
        {
            UniTask sessionTask = InitGameScene();
            await LoadingManager.Instance.LoadSceneAsync(sceneName, sessionTask);
        }

        private async UniTask InitGameScene()
        {
            await UniTask.WhenAll(
                // TODO:
                // Stage 데이터 세팅
                // Audio 프리로드
                // Chart 로드
                UniTask.CompletedTask
            );
        }
    }
}
