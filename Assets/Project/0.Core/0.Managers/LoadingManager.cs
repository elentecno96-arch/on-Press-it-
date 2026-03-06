using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Core.Managers
{
    /// <summary>
    /// 비동기 씬 전환을 도와주는 매니저
    /// </summary>
    public class LoadingManager : BaseSingleton<LoadingManager>
    {
        public event Action OnLoadingStarted;
        public event Action OnLoadingFinished;

        private const float LOAD_THRESHOLD = 0.9f;

        public override async UniTask Initialize() => await UniTask.CompletedTask;

        public async UniTask LoadSceneAsync(string sceneName)
        {
            OnLoadingStarted?.Invoke();

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            await UniTask.WaitUntil(() => op.progress >= LOAD_THRESHOLD);
            op.allowSceneActivation = true;
            await UniTask.WaitUntil(() => op.isDone);

            await InitializeStageInScene();

            OnLoadingFinished?.Invoke();
        }

        private async UniTask InitializeStageInScene()
        {
            var stageManager = UnityEngine.Object.FindFirstObjectByType<StageManager>();

            if (stageManager == null) return;

            var tcs = new UniTaskCompletionSource();

            Action onInit = null;
            onInit = () =>
            {
                stageManager.OnStageInitialized -= onInit;
                tcs.TrySetResult();
            };
            stageManager.OnStageInitialized += onInit;

            stageManager.Initialize().Forget();

            await tcs.Task;
        }
    }
}