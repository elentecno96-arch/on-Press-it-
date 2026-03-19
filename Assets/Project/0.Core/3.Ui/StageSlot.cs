using Project.Core.Managers;
using Project.Rhythm.Data;
using UnityEngine;
using UnityEngine.UI;

public class StageSlot : MonoBehaviour
{
    [SerializeField] private StageData stageData;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartStage(stageData).Forget();
        });
    }
}
