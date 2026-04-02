using Cysharp.Threading.Tasks;
using Project.Core.Ui.GlobalUi.View;
using Project.Core.Utilities;
using Project.Data.LoadingText;
using System.Threading;
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
        [SerializeField] private LoadingMessageData messageData;

        private CancellationTokenSource _fadeCts;

        public override UniTask Initialize()
        {
            fadeView.Init();
            loadingView.Init();
            IsInitialized = true;

            return UniTask.CompletedTask;
        }

        private CancellationToken GetFreshToken()
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = new CancellationTokenSource();
            return _fadeCts.Token;
        }

        public void ResetFade()
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = null;

            fadeView.SetAlphaImmediate(FADE_OUT_VALUE);
        }

        public async UniTask ShowLoading()
        {
            var token = GetFreshToken();
            await fadeView.PlayFade(FADE_IN_VALUE, -1f, token);

            if (messageData != null)
            {
                loadingView.SetText(messageData.GetRandomMessage());
            }

            loadingView.SetVisible(true);
            await loadingView.Show();
        }

        public async UniTask HideLoading()
        {
            var token = GetFreshToken();
            await loadingView.Hide();
            loadingView.SetVisible(false);

            await fadeView.PlayFade(FADE_OUT_VALUE, -1f, token);
        }

        public void SetProgress(float val)
        {
            loadingView.UpdateProgress(val, 0f).Forget();
        }

        public async UniTask FadeIn(float duration)
        {
            var token = GetFreshToken();
            await fadeView.PlayFade(FADE_IN_VALUE, duration, token);
        }

        public async UniTask FadeOut(float duration)
        {
            var token = GetFreshToken();
            await fadeView.PlayFade(FADE_OUT_VALUE, duration, token);
        }

        public async UniTask UpdateProgress(float val, float dur) => await loadingView.UpdateProgress(val, dur);
    }
}