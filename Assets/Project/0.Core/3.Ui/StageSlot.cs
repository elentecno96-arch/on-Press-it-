using System;
using Project.Rhythm.Data;
using UnityEngine;
using UnityEngine.UI;

public class StageSlot : MonoBehaviour
{
    [Header("--- Data ---")]
    [SerializeField] private StageData stageData;

    [Header("--- UI Components ---")]
    [SerializeField] private Button startButton;

    [Header("--- Lock Visuals ---")]
    [SerializeField] private GameObject lockIcon;

    // Presenter가 이 신호를 듣고 정보창을 띄울 수 있게 이벤트를 선언합니다.
    public event Action<StageData> OnSlotClicked;

    // 이 슬롯의 인덱스를 반환하는 프로퍼티
    public int StageIndex => GetStageIndex();

    private void Awake()
    {
        // 1. 버튼 클릭 리스너 초기화 메서드 호출
        SetupButtonListeners();
    } 
    /// 버튼의 클릭 이벤트를 등록하는 초기화 메서드입니다.
    private void SetupButtonListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(HandleButtonClick);
        }
    }
    /// 버튼 클릭 시 실행되는 로직을 담당하는 메서드입니다. (기존 내용 유지) 
    private void HandleButtonClick()
    {
        // "내가 눌렸어!"라고 데이터와 함께 Presenter에게 신호를 보냅니다.
        OnSlotClicked?.Invoke(stageData);

        // 클릭 로그 출력
        LogClickInfo();
    }      
    
    /// 해금 상태에 따라 UI를 업데이트하는 핵심 메서드입니다.    
    /// <param name="isUnlocked">해금 여부</param>
    public void SetUnlockState(bool isUnlocked)
    {
        // 1. 버튼 활성화/비활성화 제어
        // 이유: 잠긴 스테이지는 클릭할 수 없어야 하므로 버튼의 상호작용을 제어
        UpdateInteraction(isUnlocked);

        // 2. 자물쇠 아이콘 표시/숨김 제어
        // 이유: 잠긴 스테이지는 자물쇠 아이콘이 보여야 하므로 시각적 상태를 제어
        UpdateLockVisual(isUnlocked);
    }
   
    /// 버튼의 상호작용(interactable) 상태를 업데이트합니다.    
    private void UpdateInteraction(bool isUnlocked)
    {
        if (startButton != null)
        {
            // 설명: 유니티 Button 컴포넌트의 interactable 속성을 해금 여부에 따라 켜거나 끕
            startButton.interactable = isUnlocked;
        }
    }

    /// 자물쇠 오브젝트의 활성화 상태를 업데이트합니다.   
    private void UpdateLockVisual(bool isUnlocked)
    {
        if (lockIcon != null)
        {
            // 해금되면(true) 아이콘은 꺼짐(false)
            // 설명: 해금(isUnlocked = true)되었다면 자물쇠는 보이지 않아야 하므로 반전(!isUnlocked)
            lockIcon.SetActive(!isUnlocked);
        }
    }

    /// 현재 할당된 StageData의 인덱스를 안전하게 반환합니다.
    public int GetStageIndex()
    {
        // 이유: 데이터가 할당되지 않은 경우 -1을 반환하여 에러 상황을 인지할 수 있게 합니다.
        return stageData != null ? stageData.stageIndex : -1;
    }

    /// 개발 중 확인을 위한 로그 출력 메서드입니다.
    private void LogClickInfo()
    {
        Debug.Log($"슬롯 클릭됨: {GetStageIndex()}");
    }
}
