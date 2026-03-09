using Cysharp.Threading.Tasks;
using Project.Rhythm.Data;
using Project.Rhythm.Interface;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 범용 터치 반응 컴포넌트.
/// 스테이지별 프리팹에 부착하여 이미지 교체 또는 애니메이션 재생을 담당
/// </summary>
public class TouchEventVisual : MonoBehaviour, ITouchVisual
{
    [SerializeField] private Image targetImage;              // 변경 이미지
    [SerializeField] private Sprite idleSprite;              // 대기 이미지
    [SerializeField] private Sprite actionSprite;            // 액션 이미지
    [SerializeField] private int returnMs = 100;             // 액션 후 대기 복귀 속도

    [SerializeField] private Animator animator;              // 연출이 필요할 경우 사용하는 애니메이터
    [SerializeField] private string tapTrigger = "Tap";      // Tap 트리거 이름
    [SerializeField] private string slideTrigger = "Slide";  // Slide 트리거 이름
    [SerializeField] private string holdBool = "IsHolding";  

    private bool _isActionPlaying; // 중복 실행 방지를 위한 플래그
    private bool _isPermanentAction;

    /// <summary>
    /// (인터페이스 구현부) 터치 발생 시 호출
    /// </summary>
    public void PlayAction(PatternType type)
    {
        var token = this.GetCancellationTokenOnDestroy();

        switch (type)
        {
            case PatternType.Tap:
                HandleTapVisual(token).Forget();
                break;
            case PatternType.Slide:
                SlideVisual().Forget();
                break;
            case PatternType.Hold:
                SetHoldState(true);
                break;
        }
    }

    private async UniTaskVoid HandleTapVisual(CancellationToken token)
    {
        if (animator != null)
        {
            animator.SetTrigger(tapTrigger);
        }
        if (!_isPermanentAction)
        {
            if (targetImage != null && actionSprite != null)
            {
                targetImage.sprite = actionSprite;
            }
            _isPermanentAction = true;
    }
        await UniTask.CompletedTask;
    }

    public void StopHoldAction()
    {
        SetHoldState(false);
    }

    private async UniTaskVoid TapVisual(CancellationToken token) 
    {
        if (_isActionPlaying) return;
        _isActionPlaying = true;

        if (animator != null) animator.SetTrigger(tapTrigger);

        if (targetImage != null && actionSprite != null)
        {
            targetImage.sprite = actionSprite;

            await UniTask.Delay(returnMs, cancellationToken: token);

            if (targetImage != null)
            {
                targetImage.sprite = idleSprite;
            }
        }

        _isActionPlaying = false;
    }

    private async UniTaskVoid SlideVisual()
    {
        if (animator != null) animator.SetTrigger(slideTrigger);

        await SwapSprite();
    }

    private void SetHoldState(bool isHolding)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(holdBool, isHolding);
        }

        if (targetImage != null && actionSprite != null)
        {
            targetImage.sprite = isHolding ? actionSprite : idleSprite;
        }
    }

    private async UniTask SwapSprite()
    {
        if (targetImage != null && actionSprite != null)
        {
            targetImage.sprite = actionSprite;
            await UniTask.Delay(returnMs);
            targetImage.sprite = idleSprite;
        }
    }
}