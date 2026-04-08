using Project.Core.Managers;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.Ui.StageUi.View
{
    public class ResultView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentScoreText; // 이미지 중앙의 큰 점수 (1,000,000)
        [SerializeField] private TextMeshProUGUI perfact_T;
        [SerializeField] private TextMeshProUGUI great_T;
        [SerializeField] private TextMeshProUGUI good_T;
        [SerializeField] private TextMeshProUGUI miss_T;

        [SerializeField] private Button retry;
        [SerializeField] private Button cancel;

        [SerializeField] private TextMeshProUGUI[] bestNameTexts;  // 본인 이름 표시용 (이름)
        [SerializeField] private TextMeshProUGUI[] bestScoreTexts; // 최고 기록 점수 표시용 (10,000,000)

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

        public void DisplayResult(int score, int p, int gr, int go, int m)
        {
            _isActionStarted = false;
            gameObject.SetActive(true);

            if (currentScoreText != null) currentScoreText.text = $"{score:N0}";

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

            string myName = PlayerManager.Instance.Data.userName;

            var topRecords = PlayerManager.Instance.GetTopThreeRecords(currentStage.stageIndex);

            for (int i = 0; i < bestScoreTexts.Length; i++)
            {
                if (i < topRecords.Count)
                {
                    var record = topRecords[i];

                    if (i < bestNameTexts.Length && bestNameTexts[i] != null)
                        bestNameTexts[i].text = myName;

                    if (record.score > 0)
                    {
                        bestScoreTexts[i].text = $"{record.score:N0}";
                    }
                    else
                    {
                        bestScoreTexts[i].text = "0";
                    }
                }
                else
                {
                    if (i < bestNameTexts.Length && bestNameTexts[i] != null)
                        bestNameTexts[i].text = "-";
                    bestScoreTexts[i].text = "-";
                }
            }
        }
    }
}