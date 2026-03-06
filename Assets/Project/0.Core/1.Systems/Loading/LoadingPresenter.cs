using UnityEngine;
using Cysharp.Threading.Tasks;
using Project.Core.Managers;
using Project.Core.Ui.GlobalUi;

namespace Project.Core.Systems.Loading
{
    /// <summary>
    /// 로드의 연출 
    /// </summary>
    public class LoadingPresenter : MonoBehaviour
    {
        [SerializeField] private float fakeProgressSpeed = 2.5f;
        [SerializeField] private float minLoadDuration = 0.5f;

        private float fakeProgress;
        private float startTime;

        private void Start()
        {
            //이벤트 구독
            LoadingManager.Instance.OnLoadingStarted += LoadingStarted;
            LoadingManager.Instance.OnLoadingFinished += LoadingFinished;
        }

        private void LoadingStarted()
        {
            startTime = Time.time;
            fakeProgress = 0f;

            GlobalUIPresenter.Instance.ShowLoading().Forget();
            UpdateFakeProgress().Forget();
        }

        /// <summary>
        /// 불러오기가 빨라도 로딩 연출위한 가짜 프로세스
        /// </summary>
        /// <returns></returns>
        private async UniTaskVoid UpdateFakeProgress()
        {
            float lastSentValue = -1f;
            fakeProgress = 0f;

            GlobalUIPresenter.Instance.SetProgress(0f);

            while (fakeProgress < 1f)
            {
                fakeProgress = Mathf.MoveTowards(fakeProgress, 1f, Time.deltaTime * fakeProgressSpeed);

                if (Mathf.Abs(fakeProgress - lastSentValue) > 0.005f)
                {
                    GlobalUIPresenter.Instance.SetProgress(fakeProgress);
                    lastSentValue = fakeProgress;
                }

                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
        }

        /// <summary>
        /// 반환값이 void여야 해서 만든 함수
        /// </summary>
        private void LoadingFinished()
        {
            LoadingFinishedLogic().Forget();
        }

        /// <summary>
        /// 로딩 종료 로직
        /// </summary>
        /// <returns></returns>
        private async UniTaskVoid LoadingFinishedLogic()
        {
            await UniTask.WaitUntil(() => fakeProgress >= 1f);

            float elapsed = Time.time - startTime;
            if (elapsed < minLoadDuration)
            {
                await UniTask.Delay((int)((minLoadDuration - elapsed) * 1000));
            }

            await GlobalUIPresenter.Instance.UpdateProgress(1f, 0.1f);
            await GlobalUIPresenter.Instance.HideLoading();
        }

        /// <summary>
        /// 파괴 시 이벤트 구독 해제
        /// </summary>
        private void OnDestroy()
        {
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.OnLoadingStarted -= LoadingStarted;
                LoadingManager.Instance.OnLoadingFinished -= LoadingFinished;
            }
        }
    }
}