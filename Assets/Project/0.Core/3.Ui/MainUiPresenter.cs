using UnityEngine;

public class MainUiPresenter : MonoBehaviour
{
    [SerializeField] private MainUiView _view;

    // [추가] 선택된 스테이지 ID를 저장할 변수 선언 (오류 해결 지점)
    private int _selectedStageId = -1;

    private void OnEnable()
    {
        // 1. 스테이지 선택 -> 정보창 오픈
        _view.OnStageSelect += HandleStageSelect;
        _view.OnPlayClick += HandlePlayGame;

        // [추가] 뷰의 플레이 버튼 클릭 이벤트와 핸들러 연결
        // _view.OnPlayClick += HandlePlayGame; 

        // 2. 설정 버튼 클릭 -> (결과 정보 + 오디오 설정) 창 오픈
        _view.OnSettingsClick += () => _view.ShowSettings(true);
        _view.OnSettingsCloseClick += () => _view.ShowSettings(false);

        // 3. 홈 버튼 클릭 -> 모든 팝업 닫고 메인 뷰로 (초기화)
        _view.OnHomeClick += () => _view.ResetToMain();
    }

    private void HandleStageSelect(int stageId)
    {
        // [수정] 넘겨받은 stageId를 변수에 저장해둡니다.
        _selectedStageId = stageId;

        _view.ShowStageInfoWindow(true);
        Debug.Log($"스테이지 {stageId} 정보창 활성화 및 ID 저장 완료");
    }

    private void OnDisable()
    {
        _view.OnStageSelect -= HandleStageSelect;
        // _view.OnPlayClick -= HandlePlayGame;
    }

    // 플레이 버튼을 눌렀을 때 실행될 뼈대 함수
    private void HandlePlayGame()
    {
        // 이제 _selectedStageId를 인식할 수 있습니다.
        if (_selectedStageId == -1)
        {
            Debug.LogWarning("선택된 스테이지 정보가 없습니다.");
            return;
        }

        ExecuteStageStart(_selectedStageId);
    }

    private void ExecuteStageStart(int stageId)
    {
        // 뼈대 유지
        Debug.Log($"[Presenter] {stageId}번 스테이지 정보를 기반으로 게임 진입을 준비합니다.");
    }
}