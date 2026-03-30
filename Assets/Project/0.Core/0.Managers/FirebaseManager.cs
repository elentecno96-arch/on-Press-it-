using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
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

        Debug.Log("Firebase 초기화 중...");

        // 의존성 체크
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase 의존성 해결 불가: {dependencyStatus}");
            return;
        }

        AppOptions options = new AppOptions
        {
            DatabaseUrl = new System.Uri("https://pressit-f30f8-default-rtdb.firebaseio.com/")
        };

        FirebaseApp app = FirebaseApp.Create(options);
        Auth = FirebaseAuth.GetAuth(app);

        string databaseUrl = "https://pressit-f30f8-default-rtdb.firebaseio.com/";
        DbRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference;

        // 익명 로그인
        try
        {
            var authResult = await Auth.SignInAnonymouslyAsync().AsUniTask();
            User = authResult.User;
            Debug.Log($"Firebase 초기화 및 익명 로그인 성공! UID: {User.UserId}");
            IsInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 로그인 실패: {e.Message}");
        }
    }
}
