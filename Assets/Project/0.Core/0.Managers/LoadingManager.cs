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
        private const float MIN_LOAD_DURATION = 0.5f;      // 최소 유지 시간
        private const float FAKE_PROGRESS_SPEED = 2.5f;    // 게이지 추격 속도
        private const float PROGRESS_UPDATE_DUR = 0.02f;   // UI 업데이트 간격
        private const float FINISH_UPDATE_DUR = 0.05f;     // 최종 완료 연출 시간
        private const float LOAD_THRESHOLD = 0.9f;         // 씬 로드 완료 기준점

        public override async UniTask Initialize() => await UniTask.CompletedTask;

        public async UniTask LoadSceneAsync(string sceneName, UniTask initTask)
        {
            Debug.Log($"{sceneName}으로 로딩 시작");

            float startTime = Time.time;
            await GlobalUIPresenter.Instance.ShowLoading();

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float fakeProgress = 0f;
            while (fakeProgress < LOAD_THRESHOLD)
            {
                float realProgress = Mathf.Clamp01(op.progress / LOAD_THRESHOLD);

                fakeProgress = Mathf.MoveTowards(fakeProgress, realProgress, Time.deltaTime * FAKE_PROGRESS_SPEED);
                await GlobalUIPresenter.Instance.UpdateProgress(fakeProgress, PROGRESS_UPDATE_DUR);

                if (fakeProgress >= LOAD_THRESHOLD) break;
                await UniTask.Yield();
            }

            op.allowSceneActivation = true;
            await UniTask.WaitUntil(() => op.isDone);
            await initTask;

            float elapsed = Time.time - startTime;
            if (elapsed < MIN_LOAD_DURATION)
            {
                int delayMs = (int)((MIN_LOAD_DURATION - elapsed) * 1000);
                await UniTask.Delay(delayMs);
            }

            // 최종 완료 처리
            await GlobalUIPresenter.Instance.UpdateProgress(1f, FINISH_UPDATE_DUR);
            await GlobalUIPresenter.Instance.HideLoading();
        }
    }
}
