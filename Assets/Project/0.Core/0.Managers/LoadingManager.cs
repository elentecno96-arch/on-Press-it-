using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Core.Managers
{
    /// <summary>
    /// 비동기 씬 전환을 도와주는 매니저
    /// </summary>
    public class LoadingManager : BaseSingleton<LoadingManager>
    {
        public override async UniTask Initialize() => await UniTask.CompletedTask;

        public async UniTask LoadSceneAsync(string sceneName)
        {
            // await UIManager.Instance.FadeOut();

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                await UniTask.Yield();
            }

            // 4. [중요] 다른 매니저들의 초기화가 끝날 때까지 대기
            // 팀원이 만든 사운드 매니저나 플레이어 매니저를 여기서 기다려줍니다.
            //await UniTask.WhenAll(
            //    PlayerManager.Instance.Initialize(),
            //    AudioManager.Instance.Initialize()
            //);

            op.allowSceneActivation = true;

            // await UIManager.Instance.FadeIn();
        }
    }
}
