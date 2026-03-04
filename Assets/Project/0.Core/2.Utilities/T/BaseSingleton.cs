using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Utilities
{
    /// <summary>
    /// 제네릭 추상 싱글톤
    /// </summary>
    public abstract class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isShuttingDown = false;

        public static T Instance
        {
            get
            {
                if (_isShuttingDown) return null; //새로운 객체 생성 방지

                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        // 씬에 없다면 새로 생성 (필요에 따라 선택)
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (transform.parent != null) transform.SetParent(null); //자식 처리

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (_instance != this) Destroy(gameObject);
            }
        }

        private void OnApplicationQuit() => _isShuttingDown = true;
        private void OnDestroy() => _isShuttingDown = true;

        /// <summary>
        /// 모든 매니저가 공통으로 가질 비동기 초기화 함수
        /// </summary>
        public abstract UniTask Initialize();
    }
}
