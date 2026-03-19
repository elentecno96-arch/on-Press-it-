using System; // [필수] Action을 사용하기 위해 필요합니다.
using Project.Rhythm.Data;
using UnityEngine;
using UnityEngine.UI;

public class StageSlot : MonoBehaviour
{
    [SerializeField] private StageData stageData;
    [SerializeField] private Button startButton;

    // [추가] Presenter가 이 신호를 듣고 정보창을 띄울 수 있게 이벤트를 선언합니다.
    public event Action<StageData> OnSlotClicked;

    private void Awake()
    {
        // 버튼 클릭 시 로직 수정
        startButton.onClick.AddListener(() =>
        {
            // [수정] 직접 게임을 바로 시작하지 않고, 
            // "내가 눌렸어!"라고 데이터와 함께 Presenter에게 신호를 보냅니다.
            OnSlotClicked?.Invoke(stageData);
            Debug.Log("눌림");
        });

    }
    
}
