using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Utilities
{
    /// <summary>
    /// 제네릭 추상 베이스 싱글톤
    /// 명시적 생성 전용 (자동 생성 없음)
    /// </summary>
    public abstract class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isApplicationQuitting;

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    Debug.LogWarning($"{typeof(T).Name} accessed during application quit.");
                    return null;
                }

                if (_instance == null)
                {
                    Debug.LogError($"{typeof(T).Name} is not initialized. Make sure it exists in Bootstrap scene.");
                }

                return _instance;
            }
        }

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized { get; protected set; }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError($"{typeof(T).Name} duplicated. Destroying new instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;

            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        /// <summary>
        /// 모든 매니저가 구현해야 할 비동기 초기화
        /// </summary>
        public abstract UniTask Initialize();
    }
}
