using Project.Core.Managers;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;

namespace Project.Rhythm.Note
{
    /// <summary>
    /// 리듬 이벤트용 노트 오브젝트
    /// (타이밍과 상태만 관리)
    /// </summary>
    public class Note : MonoBehaviour
    {
        private ITouchVisual _visual;
        private RectTransform _rectTransform;

        [SerializeField] private bool isPersistent;
        [SerializeField] private string noteID;
        public bool IsPersistent => isPersistent;
        public string NoteID => noteID;
        public float SpawnTime { get; private set; }
        public float AppearDuration { get; private set; }

        private bool _isJudged;
        private bool isRandomPos;

        private void Awake()
        {
            _visual = GetComponent<ITouchVisual>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(float spawnTime, float appearDuration, bool isRandomPos = false)
        {
            SpawnTime = spawnTime;
            AppearDuration = appearDuration;
            _isJudged = false;

            if (_rectTransform != null)
            {
                if (isRandomPos)
                {
                    float rx = Random.Range(-400f, 400f);
                    float ry = Random.Range(-200f, 200f);
                    _rectTransform.anchoredPosition = new Vector2(rx, ry);
                }
                else
                {
                    _rectTransform.anchoredPosition = Vector2.zero;
                }

                Vector3 lp = _rectTransform.localPosition;
                lp.z = 0f;
                _rectTransform.localPosition = lp;
            }
        }

        public void UpdateNote(float currentTime)
        {
            if (isPersistent) return;

            float elapsed = currentTime - SpawnTime;
            float progress = elapsed / AppearDuration;

            _visual?.UpdateVisual(progress);

            if (progress >= 1.5f)
            {
                HandleRetire();
            }
        }

        public void PlaySignalEffect()
        {
            // ITouchVisual에 Signal용 액션을 추가하거나 특정 트리거를 실행
            // _visual?.PlayAction(PatternType.Signal); // 가상의 연출 타입
        }

        public void OnJudged(JudgeResult result)
        {
            if (_isJudged) return;
            _isJudged = true;

            _visual?.PlayAction(result);

            if (!isPersistent)
            {
                Invoke(nameof(HandleRetire), 0.5f);
            }
        }

        private void HandleRetire()
        {
            if (isPersistent) return; 
            Destroy(gameObject);
        }

        public void UpdateHoldProgress(float progress)
        {
            _visual?.UpdateVisual(progress);
        }

        public void ResetJudgedState()
        {
            _isJudged = false;
            // 비주얼도 초기 상태로 리셋 (정적 노트 재사용 시 필수)
            // _visual?.ResetVisual(); 
        }
    }
}
