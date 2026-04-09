using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Project.Core.Managers;
using Project.Core.Ui.GlobalUi;
using Project.Core.Utilities;
using UnityEngine;

public class FirebaseManager : BaseSingleton<FirebaseManager>
{
    public DatabaseReference DbRef { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }

    public override async UniTask Initialize()
    {
        if (IsInitialized) return;

        Debug.Log("Firebase 초기화 시작...");

        // 의존성 체크 및 기본 앱 초기화 (google-services.json 참조)
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase 의존성 해결 불가: {dependencyStatus}");
            return;
        }

        Auth = FirebaseAuth.DefaultInstance;
        DbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // 익명 로그인 시도
        try
        {
            var authResult = await Auth.SignInAnonymouslyAsync().AsUniTask();
            User = authResult.User;

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.SetUserIdentity(User.UserId);
            }

            await UniTask.SwitchToMainThread();
            if (GlobalUIPresenter.Instance != null)
            {
                GlobalUIPresenter.Instance.ShowNotification("서버 연결 성공");
            }

            Debug.Log($"<color=green>Firebase 인증 성공! UID: {User.UserId}</color>");
            IsInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 실제 로그인 실패 원인: {e.GetBaseException().Message}");

            if (GlobalUIPresenter.Instance != null)
            {
                GlobalUIPresenter.Instance.ShowNotification("서버 연결 실패. 오프라인 모드로 전환합니다.");
            }
        }
    }

    public void SaveDataWithCheck(string path, string json)
    {
        if (!IsInitialized || User == null) return;
        SaveDataAsync(path, json).Forget();
    }

    private async UniTaskVoid SaveDataAsync(string path, string json, bool showNotification = false)
    {
        try
        {
            await DbRef.Child(path).SetRawJsonValueAsync(json)
                .AsUniTask()
                .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());

            Debug.Log($"<color=cyan>[Firebase] 저장 성공: {path}</color>");

            if (showNotification)
            {
                if (GlobalUIPresenter.Instance != null)
                {
                    GlobalUIPresenter.Instance.ShowNotification("데이터가 서버에 안전하게 저장되었습니다.");
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[Firebase] 저장 작업이 취소되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] 저장 실패: {e.Message}");

            if (GlobalUIPresenter.Instance != null)
            {
                GlobalUIPresenter.Instance.ShowNotification("서버 저장에 실패했습니다. 네트워크를 확인해주세요.");
            }
        }
    }

    public async UniTask SavePlayerData(string json)
    {
        if (!IsInitialized || User == null) return;
        try
        {
            await DbRef.Child("users").Child(User.UserId).Child("playerData").SetRawJsonValueAsync(json).AsUniTask();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] PlayerData 저장 실패: {e.Message}");
            if (GlobalUIPresenter.Instance != null)
            {
                GlobalUIPresenter.Instance.ShowNotification("서버 저장에 실패했습니다. 네트워크를 확인해주세요.");
            }
            throw;
        }
    }

    public async UniTask<string> LoadPlayerData()
    {
        if (!IsInitialized || User == null) return null;

        try
        {
            var snapshot = await DbRef
                .Child("users")
                .Child(User.UserId)
                .Child("playerData")
                .GetValueAsync().AsUniTask();

            return snapshot.Exists ? snapshot.GetRawJsonValue() : null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseManager] 데이터 로드 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 특정 값을 저장하는 매서드
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async UniTask SetValueAsync(string path, object value)
    {
        if (!IsInitialized || User == null) return;
        await DbRef.Child(path).SetValueAsync(value).AsUniTask();
    }

    /// <summary>
    /// 랭킹용 데이터 업데이트
    /// </summary>
    /// <param name="nickname"></param>
    /// <param name="totalScore"></param>
    public void UpdateLeaderboardData(string nickname, float totalScore)
    {
        if (!IsInitialized || User == null) return;

        string path = $"leaderboard/{User.UserId}";
        var data = new System.Collections.Generic.Dictionary<string, object>
        {
            { "userName", nickname },
            { "score", totalScore },
            { "lastUpdated", ServerValue.Timestamp }
        };
        DbRef.Child(path).UpdateChildrenAsync(data).AsUniTask().Forget();
    }

    /// <summary>
    /// 내 랭킹과 백분율을 한 번에 가져와서 PlayerManager에 전달합니다.
    /// </summary>
    public async UniTask RefreshMyRankData(float myTotalScore)
    {
        if (!IsInitialized || User == null || myTotalScore <= 0) return;

        try
        {
            var rankSnapshot = await DbRef.Child("leaderboard")
                .OrderByChild("score")
                .StartAt(myTotalScore + 0.0001f)
                .GetValueAsync().AsUniTask();

            int myRank = (int)rankSnapshot.ChildrenCount + 1;

            var totalSnapshot = await DbRef.Child("leaderboard").GetValueAsync().AsUniTask();
            long totalUsers = totalSnapshot.ChildrenCount;

            float percent = totalUsers > 0 ? (float)myRank / totalUsers * 100f : 0f;

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.UpdateRank(myRank, percent);
            }

            Debug.Log($"[Firebase] 랭킹 갱신 완료: {myRank}위 / 전체 {totalUsers}명 (상위 {percent:F1}%)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] 랭킹 데이터 갱신 실패: {e.Message}");
        }
    }
}
