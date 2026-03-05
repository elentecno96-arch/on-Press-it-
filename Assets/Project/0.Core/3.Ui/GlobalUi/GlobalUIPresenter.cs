using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using Project.Core.Ui.GlobalUi.View;
using UnityEngine;

namespace Project.Core.Ui.GlobalUi
{
    /// <summary>
    /// 글로벌 Ui 중재자
    /// </summary>
    public class GlobalUIPresenter : BaseSingleton<GlobalUIPresenter>
    {
        [SerializeField] private GlobalFadeView fadeView;       
        [SerializeField] private GlobalLoadingView loadingView; 

        // 추후 SO로 분리 예정
        private readonly string[] _messages = { "화면 닦는 중...", "노트 배치 중...", "관객 입장 중..." };

        public override UniTask Initialize()
        {
            fadeView.Init();
            loadingView.Init();
            return UniTask.CompletedTask;
        }

        public async UniTask ShowLoading()
        {
            await fadeView.PlayFade(1f);

            string randomMsg = _messages[Random.Range(0, _messages.Length)];
            loadingView.SetText(randomMsg);
            loadingView.SetVisible(true);

            await loadingView.Show(); 
        }

        public async UniTask HideLoading()
        {
            await loadingView.Hide(); 
            loadingView.SetVisible(false);

            await fadeView.PlayFade(0f);
        }

        public void SetProgress(float val)
        {
            loadingView.UpdateProgress(val, 0f).Forget();
        }

        public async UniTask UpdateProgress(float val, float dur) => await loadingView.UpdateProgress(val, dur);
    }
}
