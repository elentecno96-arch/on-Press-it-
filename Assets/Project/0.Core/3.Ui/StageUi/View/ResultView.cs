using Project.Core.Managers;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.Ui.StageUi.View
{
    /// <summary>
    /// Game씬의 결과창 Ui View
    /// </summary>
    public class ResultView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI perfact_T;
        [SerializeField] private TextMeshProUGUI great_T;
        [SerializeField] private TextMeshProUGUI good_T;
        [SerializeField] private TextMeshProUGUI miss_T;

        [SerializeField] private Button retry;
        [SerializeField] private Button cancel;

        [SerializeField] private TextMeshProUGUI[] bestScoreTexts; // 각 등급별 최고 기록을 표시할 텍스트 배열

        private bool _isActionStarted = false;

        private void Awake()
        {
            retry.onClick.AddListener(() => {
                if (_isActionStarted) return;
                _isActionStarted = true;

                var currentData = GameManager.Instance.CurrentStageData;
                GameManager.Instance.StartStage(currentData).Forget();
            });

            cancel.onClick.AddListener(() => {
                if (_isActionStarted) return;
                _isActionStarted = true;

                LoadingManager.Instance.LoadSceneAsync("Main").Forget();
            });

            gameObject.SetActive(false);
        }

        public void DisplayResult(int p, int gr, int go, int m)
        {
            _isActionStarted = false;
            gameObject.SetActive(true);

            perfact_T.text = p.ToString();
            great_T.text = gr.ToString();
            good_T.text = go.ToString();
            miss_T.text = m.ToString();

            ShowPersonalBestRecords();
        }

        private void ShowPersonalBestRecords()
        {
            var currentStage = GameManager.Instance.CurrentStageData;
            if (currentStage == null) return;

            var topRecords = PlayerManager.Instance.GetTopThreeRecords(currentStage.stageIndex);

            for (int i = 0; i < bestScoreTexts.Length; i++)
            {
                if (i < topRecords.Count)
                {
                    var record = topRecords[i];
                    bestScoreTexts[i].text = $"{record.score:N0} : {record.date}";
                }
                else
                {
                    bestScoreTexts[i].text = "-";
                }
            }
        }
    }
}
