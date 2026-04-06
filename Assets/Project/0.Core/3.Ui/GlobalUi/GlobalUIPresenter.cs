using Cysharp.Threading.Tasks;
using Project.Core.Ui.GlobalUi.View;
using Project.Core.Utilities;
using Project.Data.LoadingText;
using System.Collections.Generic;
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
        [SerializeField] private NotificationView notificationView;
        [SerializeField] private LoadingMessageData messageData;

        //Queue 순차 적 팝업 알람 ( 현재는 즉시 표시 )
        //private Queue<string> _notiQueue = new ();
        //private bool _isShowing;

        private CancellationTokenSource _fadeCts;
        private CancellationTokenSource _notiCts;
        public override UniTask Initialize()
        {
            fadeView.Init();
            loadingView.Init();

            if (notificationView != null)
                notificationView.Init();

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

        /// <summary>
        /// 화면에 짧은 알림 메시지를 띄웁니다.
        /// </summary>
        /// <param name="message">표시할 내용</param>
        /// <param name="duration">유지 시간 (기본 2초)</param>
        public void ShowNotification(string message, float duration = 2.0f)
        {
            if (notificationView == null) return;
            Debug.Log("팝업 호출됨");

            _notiCts?.Cancel();
            _notiCts?.Dispose();
            _notiCts = new CancellationTokenSource();

            notificationView.ShowMessage(message, duration, _notiCts.Token).Forget();
        }

        public async UniTask UpdateProgress(float val, float dur) => await loadingView.UpdateProgress(val, dur);
    }
}