using Project.Core.Managers;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;

namespace Project.Rhythm.Note
{

    public enum NoteType
    {
        Runtime,
        Persistent,
        Signal
    }

    /// <summary>
    /// 리듬 이벤트용 노트 오브젝트
    /// (타이밍과 상태만 관리)
    /// </summary>
    public class Note : MonoBehaviour
    {
        private ITouchVisual _visual;
        private RectTransform _rectTransform;

        [SerializeField] private string noteID;
        public string NoteID => noteID;

        [SerializeField] private NoteType noteType;
        public NoteType Type => noteType;
        public bool IsPersistent => noteType == NoteType.Persistent;

        public float SpawnTime { get; private set; }
        public float AppearDuration { get; private set; }

        private bool _isJudged;
        public bool Judged => _isJudged;

        private void Awake()
        {
            _visual = GetComponent<ITouchVisual>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(float spawnTime, float appearDuration)
        {
            if (noteType != NoteType.Runtime)
            {
                Debug.LogError($"[Note] Setup 잘못 호출됨: {noteType}");
                return;
            }

            SpawnTime = spawnTime;
            AppearDuration = appearDuration;
            _isJudged = false;
        }

        public void InitializePersistent(float spawnTime, float appearDuration)
        {
            if (noteType != NoteType.Persistent)
            {
                Debug.LogError($"[Note] InitializePersistent 잘못 호출됨: {noteType}");
                return;
            }

            SpawnTime = spawnTime;
            AppearDuration = appearDuration;
            _isJudged = false;
        }

        public void SetPosition(Vector2 pos)
        {
            if (_rectTransform == null) return;

            _rectTransform.anchoredPosition = pos;

            Vector3 lp = _rectTransform.localPosition;
            lp.z = 0f;
            _rectTransform.localPosition = lp;
        }

        public void UpdateNote(float currentTime)
        {
            if (noteType != NoteType.Runtime) return;

            float elapsed = currentTime - SpawnTime;
            float progress = (currentTime - SpawnTime) / AppearDuration;

            _visual?.UpdateVisual(progress);

            if (progress >= 2f)
            {
                HandleRetire();
            }
        }
        public void UpdateHoldProgress(float progress)
        {
            // 홀드 게이지를 채우거나 이펙트를 재생하는 로직
            Debug.Log($"Hold Progress: {progress}");
        }


        public void OnJudged(JudgeResult result)
        {
            if (_isJudged && IsPersistent == false) return;

            if (this == null || gameObject == null) return;

            _isJudged = true;
            _visual?.PlayAction(result);
        }

        public void ResetJudgedState()
        {
            _isJudged = false;
        }

        public void PlaySignal()
        {
            _isJudged = false;
            if (noteType != NoteType.Signal) return;

            _visual?.PlayAction(PatternType.None);
        }

        private void HandleRetire()
        {
            if (noteType != NoteType.Runtime) return;

            Destroy(gameObject);
        }
    }
}
