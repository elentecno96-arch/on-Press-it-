using Cysharp.Threading.Tasks;
using Project.Core.Utilities;

namespace Project.Core.Managers
{
    /// <summary>
    /// 플레이어의 정보를 저장하는 매니저
    /// </summary>
    public class PlayerManager : BaseSingleton<PlayerManager>
    {
        public override async UniTask Initialize()
        {
            await UniTask.Yield();

            if (IsInitialized) return;
        }
    }
}
