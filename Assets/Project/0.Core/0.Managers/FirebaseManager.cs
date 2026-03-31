using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
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

            Debug.Log($"<color=green>Firebase 인증 성공! UID: {User.UserId}</color>");
            IsInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 로그인 실패: {e.GetBaseException().Message}");
        }
    }

    public void SaveDataWithCheck(string path, string json)
    {
        if (!IsInitialized || User == null) return;
        SaveDataAsync(path, json).Forget();
    }

    private async UniTaskVoid SaveDataAsync(string path, string json)
    {
        try
        {
            await DbRef.Child(path).SetRawJsonValueAsync(json)
                .AsUniTask() 
                .AttachExternalCancellation(this.GetCancellationTokenOnDestroy()); // 여기서 토큰 연결

            Debug.Log($"<color=cyan>[Firebase] 저장 성공: {path}</color>");
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[Firebase] 저장 작업이 취소되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] 저장 실패: {e.Message}");
        }
    }

    public void SavePlayerData(string json)
    {
        if (!IsInitialized || User == null) return;

        string path = $"users/{User.UserId}/playerData";
        SaveDataAsync(path, json).Forget();
    }

}
