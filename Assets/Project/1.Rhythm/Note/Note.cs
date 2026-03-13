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
        }

        public void Setup(float spawnTime, float appearDuration, bool isRandomPos = false) 
        {
            SpawnTime = spawnTime;
            AppearDuration = appearDuration;
            this.isRandomPos = isRandomPos;
            _isJudged = false;

            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                if (this.isRandomPos)
                {
                    float rx = Random.Range(-400f, 400f);
                    float ry = Random.Range(-200f, 200f);
                    rect.anchoredPosition = new Vector2(rx, ry);
                }
                else
                {
                    rect.anchoredPosition = Vector2.zero;
                }
                Vector3 lp = rect.localPosition;
                lp.z = 0f;
                rect.localPosition = lp;

                //rect.localScale = new Vector3(0.2f, 0.2f, 1f);
            }
        }

        public void UpdateNote(float currentTime)
        {
            float elapsed = currentTime - SpawnTime;
            float progress = elapsed / AppearDuration;

            _visual?.UpdateVisual(progress);

            if (progress >= 1.5f)
            {
                Destroy(gameObject);
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
    }
}
