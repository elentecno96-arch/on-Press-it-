using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 범용 터치 반응 컴포넌트.
/// 스테이지별 프리팹에 부착하여 이미지 교체 또는 애니메이션 재생을 담당
/// </summary>
public class TouchEventVisual : MonoBehaviour, ITouchVisual
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite idleSprite;    // 대기 상태
    [SerializeField] private Sprite successSprite; // 성공 상태
    [SerializeField] private Sprite missSprite;    // 실패 상태

    [SerializeField] private Animator animator;
    [SerializeField] private string successTrigger = "Success";
    [SerializeField] private string tapTrigger = "Tap";
    [SerializeField] private string slideTrigger = "Slide";
    [SerializeField] private string holdBool = "IsHolding";

    private bool _isJudged;

    /// <summary>
    /// [노트 전용] 판정 결과가 나왔을 때 호출 (선반 연출 등)
    /// </summary>
    public void PlayAction(JudgeResult result)
    {
        if (_isJudged) return;

        if (result != JudgeResult.Miss) SetSuccessVisual();
        else SetMissVisual();
    }

    /// <summary>
    /// [플레이어 전용] 입력 액션이 발생했을 때 호출 (손 움직임 등)
    /// </summary>
    public void PlayAction(PatternType type)
    {
        if (animator == null) return;

        switch (type)
        {
            case PatternType.Tap:
                if (HasParameter(tapTrigger)) animator.SetTrigger(tapTrigger);
                break;
            case PatternType.Slide:
                if (HasParameter(slideTrigger)) animator.SetTrigger(slideTrigger);
                break;
            case PatternType.Hold:
                if (HasParameter(holdBool)) animator.SetBool(holdBool, true);
                break;
        }
    }

    // 파라미터가 실제로 애니메이터에 있는지 확인하는 헬퍼 함수
    private bool HasParameter(string paramName)
    {
        if (string.IsNullOrEmpty(paramName)) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    /// <summary>
    /// [플레이어 전용] 홀드 입력이 끝났을 때 호출
    /// </summary>
    public void StopHoldAction()
    {
        if (animator != null) animator.SetBool(holdBool, false);
    }

    private void SetSuccessVisual()
    {
        _isJudged = true;
        if (targetImage != null && successSprite != null)
        {
            targetImage.sprite = successSprite;
            targetImage.SetNativeSize();
        }

        if (animator != null) animator.SetTrigger(successTrigger);
    }

    private void SetMissVisual()
    {
        _isJudged = true;
        if (targetImage != null && missSprite != null)
        {
            targetImage.sprite = missSprite;
            targetImage.SetNativeSize();
        }
    }

    public void UpdateVisual(float progress)
    {
        transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, progress);
    }

    public void ResetVisual()
    {
        _isJudged = false;
        if (targetImage != null)
        {
            targetImage.sprite = idleSprite;
            targetImage.SetNativeSize();
        }
        if (animator != null) animator.Rebind();
        transform.localScale = Vector3.zero;
    }
}