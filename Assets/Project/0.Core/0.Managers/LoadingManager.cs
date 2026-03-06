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
            Debug.Log($"{sceneName} 로딩 시작");

            OnLoadingStarted?.Invoke();

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            await UniTask.WaitUntil(() => op.progress >= LOAD_THRESHOLD);

            op.allowSceneActivation = true;

            await UniTask.WaitUntil(() => op.isDone);

            //GameScene 초기화
            await InitializeGameScene();

            OnLoadingFinished?.Invoke();
        }

        private async UniTask InitializeGameScene()
        {
            var stageManager = UnityEngine.Object.FindFirstObjectByType<StageManager>();

            if (stageManager == null)
            {
                Debug.LogWarning("StageManager not found");
                return;
            }

            var tcs = new UniTaskCompletionSource();

            stageManager.OnStageInitialized += () =>
            {
                tcs.TrySetResult();
            };

            stageManager.Initialize().Forget();

            await tcs.Task;
        }
    }
}