using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonController : MonoBehaviour
{
    // 버튼 클릭 시 외부(Presenter)에서 알 수 있도록 이벤트 선언
    public event Action OnPlayRequested;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        // 내부 로직 없이, 플레이가 요청되었다는 사실만 알림
        Debug.Log("[PlayButton] 플레이 요청 이벤트 발생");
        OnPlayRequested?.Invoke();
    }
}
