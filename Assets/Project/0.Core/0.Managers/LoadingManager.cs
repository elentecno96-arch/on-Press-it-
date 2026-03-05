using Cysharp.Threading.Tasks;
using Project.Core.Ui.GlobalUi;
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

        public async UniTask LoadSceneAsync(string sceneName, UniTask initTask)
        {
            await GlobalUIPresenter.Instance.ShowLoading();
            GlobalUIPresenter.Instance.SetProgress(0f);

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                await GlobalUIPresenter.Instance.UpdateProgress(progress, 0.1f);
                await UniTask.Yield();
            }

            await GlobalUIPresenter.Instance.UpdateProgress(0.95f, 0.2f);

            op.allowSceneActivation = true;
            await UniTask.WaitUntil(() => op.isDone);

            await UniTask.Yield(); // 안전 프레임 확보

            await initTask;

            await GlobalUIPresenter.Instance.UpdateProgress(1f, 0.15f);
            await GlobalUIPresenter.Instance.HideLoading();
        }
    }
}
