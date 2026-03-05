using Cysharp.Threading.Tasks;
using Project.Core.Ui.GlobalUi.View;
using Project.Core.Utilities;
using Project.Data.LoadingText;
using UnityEngine;

namespace Project.Core.Ui.GlobalUi
{
    /// <summary>
    /// 글로벌 Ui 중재자
    /// </summary>
    public class GlobalUIPresenter : BaseSingleton<GlobalUIPresenter>
    {
        private const float FADE_IN_VALUE = 1f;
        private const float FADE_OUT_VALUE = 0f;

        [SerializeField] private GlobalFadeView fadeView;       
        [SerializeField] private GlobalLoadingView loadingView;

        // 추후 SO로 분리 예정
        [SerializeField] private LoadingMessageData messageData;

        public override UniTask Initialize()
        {
            fadeView.Init();
            loadingView.Init();

            IsInitialized = true;

            return UniTask.CompletedTask;
        }

        public async UniTask ShowLoading()
        {
            await fadeView.PlayFade(FADE_IN_VALUE);

            if (messageData != null)
            {
                loadingView.SetText(messageData.GetRandomMessage());
            }

            loadingView.SetVisible(true);
            await loadingView.Show();
        }

        public async UniTask HideLoading()
        {
            await loadingView.Hide(); 
            loadingView.SetVisible(false);

            await fadeView.PlayFade(FADE_OUT_VALUE);
        }

        public void SetProgress(float val)
        {
            loadingView.UpdateProgress(val, 0f).Forget();
        }

        public async UniTask UpdateProgress(float val, float dur) => await loadingView.UpdateProgress(val, dur);
    }
}
