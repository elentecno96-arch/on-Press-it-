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

            float progress = (currentTime - SpawnTime) / AppearDuration;

            _visual?.UpdateVisual(progress);

            if (progress >= 2f)
            {
                HandleRetire();
            }
        }

        public void OnJudged(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            _visual?.PlayAction(result);
        }

        public void ResetJudgedState()
        {
            _isJudged = false;
        }

        public void UpdateHoldProgress(float progress)
        {
            _visual?.UpdateVisual(progress);
        }

        public void PlaySignal()
        {
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