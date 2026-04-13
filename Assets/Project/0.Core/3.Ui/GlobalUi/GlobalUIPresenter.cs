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

        private readonly Queue<string> _notiQueue = new ();
        private bool _isShowing;

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

        private CancellationTokenSource CreateLinkedFadeTokenSource(CancellationToken externalToken)
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = new CancellationTokenSource();

            return CancellationTokenSource.CreateLinkedTokenSource(_fadeCts.Token, externalToken);
        }

        public void ResetFade()
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = null;

            fadeView.SetAlphaImmediate(FADE_OUT_VALUE);
        }

        public async UniTask ShowLoading(CancellationToken token = default)
        {
            using (var linkedSource = CreateLinkedFadeTokenSource(token))
            {
                await fadeView.PlayFade(FADE_IN_VALUE, -1f, linkedSource.Token);

                if (messageData != null)
                    loadingView.SetText(messageData.GetRandomMessage());

                loadingView.SetVisible(true);
                await loadingView.Show();
            }
        }

        public async UniTask HideLoading(CancellationToken token = default)
        {
            using (var linkedSource = CreateLinkedFadeTokenSource(token))
            {
                await loadingView.Hide();
                loadingView.SetVisible(false);

                await fadeView.PlayFade(FADE_OUT_VALUE, -1f, linkedSource.Token);
            }
        }

        public void SetProgress(float val)
        {
            loadingView.UpdateProgress(val, 0f).Forget();
        }

        public async UniTask FadeIn(float duration, CancellationToken token = default)
        {
            using (var linkedSource = CreateLinkedFadeTokenSource(token))
            {
                await fadeView.PlayFade(FADE_IN_VALUE, duration, linkedSource.Token);
            }
        }

        public async UniTask FadeOut(float duration, CancellationToken token = default)
        {
            using (var linkedSource = CreateLinkedFadeTokenSource(token))
            {
                await fadeView.PlayFade(FADE_OUT_VALUE, duration, linkedSource.Token);
            }
        }

        /// <summary>
        /// 화면에 짧은 알림 메시지를 띄웁니다.
        /// </summary>
        /// <param name="message">표시할 내용</param>
        /// <param name="duration">유지 시간 (기본 2초)</param>
        public void ShowNotification(string message, float duration = 2.0f)
        {
            if (notificationView == null) return;

            _notiQueue.Enqueue(message);

            if (!_isShowing)
            {
                ProcessNotificationQueue(duration).Forget();
            }
        }

        private async UniTaskVoid ProcessNotificationQueue(float duration)
        {
            _isShowing = true;

            while (_notiQueue.Count > 0)
            {
                string msg = _notiQueue.Dequeue();

                _notiCts?.Cancel();
                _notiCts?.Dispose();
                _notiCts = new CancellationTokenSource();

                await notificationView.ShowMessage(msg, duration, _notiCts.Token);
                await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
            }

            _isShowing = false;
        }

        private void OnDestroy()
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _notiCts?.Cancel();
            _notiCts?.Dispose();
        }

        public async UniTask UpdateProgress(float val, float dur) => await loadingView.UpdateProgress(val, dur);
    }
}