using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using System;
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
        private Action<Note> _onRelease;

        public float SpawnTime { get; private set; }
        public float AppearDuration { get; private set; }
        private bool _isReleased;
        private bool _isJudged;
        public bool Judged => _isJudged;

        private void Awake()
        {
            _visual = GetComponent<ITouchVisual>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(float spawnTime, float appearDuration)
        {
            _isReleased = false;
            _isJudged = false;
            SpawnTime = spawnTime;
            AppearDuration = appearDuration;

            if (TryGetComponent<RectTransform>(out var rt))
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            _visual?.ResetVisual();
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
            if (noteType == NoteType.Signal || _isReleased) return;
            if (_isJudged && IsPersistent) return;

            float progress = (currentTime - SpawnTime) / AppearDuration;
            _visual?.UpdateVisual(progress);

            if (noteType == NoteType.Runtime && progress >= 2.0f)
            {
                HandleRetire();
            }
        }

        public void UpdateHoldProgress(float progress)
        {
            // 홀드 게이지를 채우거나 이펙트를 재생하는 로직
        }

        public void SetPoolAction(System.Action<Note> action)
        {
            _onRelease = action;
            _isReleased = false; 
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

            _visual?.PlayAction(PatternType.Signal);
            //_visual?.PlayAction(PatternType.None);
        }

        private void HandleRetire()
        {
            if (noteType != NoteType.Runtime || _isReleased) return;

            if (_onRelease != null)
            {
                _isReleased = true;
                _onRelease.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
