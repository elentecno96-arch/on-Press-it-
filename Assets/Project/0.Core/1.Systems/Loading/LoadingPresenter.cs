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
        private float targetProgress;
        private float currentDisplayProgress;

        private void Start()
        {
            //이벤트 구독
            LoadingManager.Instance.OnLoadingStarted += LoadingStarted;
            LoadingManager.Instance.OnLoadingFinished += LoadingFinished;
            LoadingManager.Instance.OnProgressUpdated += UpdateProgressValue;
        }

        private void LoadingStarted()
        {
            targetProgress = 0f;
            currentDisplayProgress = 0f;
            GlobalUIPresenter.Instance.ShowLoading().Forget();
            SmoothUpdateUI().Forget();
        }

        private void UpdateProgressValue(float value)
        {
            targetProgress = value; 
        }

        private async UniTaskVoid SmoothUpdateUI()
        {
            while (this != null)
            {
                currentDisplayProgress = Mathf.Lerp(currentDisplayProgress, targetProgress, Time.deltaTime * 5f);
                GlobalUIPresenter.Instance.SetProgress(currentDisplayProgress);

                if (currentDisplayProgress >= 0.99f && targetProgress >= 1f) break;

                await UniTask.Yield();
            }
        }

        private void LoadingFinished()
        {
            GlobalUIPresenter.Instance.SetProgress(1f);
            GlobalUIPresenter.Instance.HideLoading().Forget();
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
                LoadingManager.Instance.OnProgressUpdated += UpdateProgressValue;
            }
        }
    }
}