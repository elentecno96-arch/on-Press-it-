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
        public event Action<float> OnProgressUpdated;

        private const float LOAD_THRESHOLD = 0.9f;
        [SerializeField] private int fadeDurationMs = 500;

        public override async UniTask Initialize()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            await BeginTransition();

            await PerformSceneLoad(sceneName);

            OnLoadingFinished?.Invoke();
            await UniTask.Delay(fadeDurationMs);
        }

        public async UniTask LoadStageAsync(string sceneName)
        {
            await BeginTransition();

            await PerformSceneLoad(sceneName);

            var stageManager = UnityEngine.Object.FindFirstObjectByType<StageManager>();
            if (stageManager != null)
            {
                await stageManager.Initialize();
            }

            OnLoadingFinished?.Invoke();
            await UniTask.Delay(fadeDurationMs);

            stageManager?.Play();
        }

        private async UniTask BeginTransition()
        {
            OnLoadingStarted?.Invoke();
            await UniTask.Delay(fadeDurationMs);
        }

        // 실제 씬 로딩 과정
        private async UniTask PerformSceneLoad(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < LOAD_THRESHOLD)
            {
                float normalizedProgress = op.progress / LOAD_THRESHOLD;
                OnProgressUpdated?.Invoke(normalizedProgress);

                await UniTask.Yield();
            }

            OnProgressUpdated?.Invoke(1.0f);

            await UniTask.Delay(200);

            op.allowSceneActivation = true;
            await UniTask.WaitUntil(() => op.isDone);
            await UniTask.NextFrame();
        }
    }
}