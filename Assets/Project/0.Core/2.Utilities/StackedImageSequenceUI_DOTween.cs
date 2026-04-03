using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class StoryImageSequenceUI_DOTween : MonoBehaviour, IPointerClickHandler
{
    private enum SequenceStep
    {
        None = 0,

        Show1,
        Show2,
        Show3,

        Fade123_Show4,
        Show5,

        Fade45_Show6,
        Show7,

        FadeAll_Disable,
        Finished
    }

    [Header("스토리 이미지 7장")]
    [SerializeField] private Image[] images = new Image[7];

    [Header("애니메이션 설정")]
    [Min(0f)][SerializeField] private float slideDuration = 0.6f;
    [Min(0f)][SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;
    [SerializeField] private Ease fadeEase = Ease.OutCubic;

    [Header("슬라이드 설정")]
    [SerializeField] private float startOffsetY = -1920f;

    [Header("재생관련")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool ignoreClickWhileAnimating = true;

    private RectTransform[] rects;
    private CanvasGroup[] canvasGroups;

    private Sequence currentSequence;
    private SequenceStep currentStep = SequenceStep.None;
    private bool isAnimating = false;

    private void Awake()
    {
        Initialize();
        HideAllImmediate();
    }

    private void OnEnable()
    {
        if (!playOnEnable)
            return;

        RestartSequence();
    }

    private void OnDisable()
    {
        KillCurrentSequence();
    }

    private void OnDestroy()
    {
        KillCurrentSequence();
    }

    private void Initialize()
    {
        if (images == null || images.Length == 0)
            return;

        rects = new RectTransform[images.Length];
        canvasGroups = new CanvasGroup[images.Length];

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null)
                continue;

            rects[i] = images[i].rectTransform;

            CanvasGroup cg = images[i].GetComponent<CanvasGroup>();
            if (cg == null)
                cg = images[i].gameObject.AddComponent<CanvasGroup>();

            canvasGroups[i] = cg;
        }
    }

    public void RestartSequence()
    {
        KillCurrentSequence();
        HideAllImmediate();

        currentStep = SequenceStep.Show1;
        isAnimating = false;

        ExecuteCurrentStep();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (ignoreClickWhileAnimating && isAnimating)
            return;

        AdvanceStep();
        ExecuteCurrentStep();
    }

    private void AdvanceStep()
    {
        switch (currentStep)
        {
            case SequenceStep.Show1:
                currentStep = SequenceStep.Show2;
                break;

            case SequenceStep.Show2:
                currentStep = SequenceStep.Show3;
                break;

            case SequenceStep.Show3:
                currentStep = SequenceStep.Fade123_Show4;
                break;

            case SequenceStep.Fade123_Show4:
                currentStep = SequenceStep.Show5;
                break;

            case SequenceStep.Show5:
                currentStep = SequenceStep.Fade45_Show6;
                break;

            case SequenceStep.Fade45_Show6:
                currentStep = SequenceStep.Show7;
                break;

            case SequenceStep.Show7:
                currentStep = SequenceStep.FadeAll_Disable;
                break;

            case SequenceStep.FadeAll_Disable:
                currentStep = SequenceStep.Finished;
                break;
        }
    }

    private void ExecuteCurrentStep()
    {
        switch (currentStep)
        {
            case SequenceStep.Show1:
                PlayShowSingle(0);
                break;

            case SequenceStep.Show2:
                PlayShowSingle(1);
                break;

            case SequenceStep.Show3:
                PlayShowSingle(2);
                break;

            case SequenceStep.Fade123_Show4:
                PlayFadeGroupAndShow(new[] { 0, 1, 2 }, 3);
                break;

            case SequenceStep.Show5:
                PlayShowSingle(4);
                break;

            case SequenceStep.Fade45_Show6:
                PlayFadeGroupAndShow(new[] { 3, 4 }, 5);
                break;

            case SequenceStep.Show7:
                PlayShowSingle(6);
                break;

            case SequenceStep.FadeAll_Disable:
                PlayFadeAllAndDisable();
                break;
        }
    }

    private void PlayShowSingle(int imageIndex)
    {
        if (!IsValidIndex(imageIndex) || images[imageIndex] == null)
            return;

        KillCurrentSequence();
        isAnimating = true;

        Image image = images[imageIndex];
        RectTransform rt = rects[imageIndex];
        CanvasGroup cg = canvasGroups[imageIndex];

        image.gameObject.SetActive(true);
        image.transform.SetAsLastSibling();

        rt.anchoredPosition = new Vector2(0f, startOffsetY);
        cg.alpha = 0f;

        currentSequence = DOTween.Sequence().SetUpdate(true);

        currentSequence.Join(rt.DOAnchorPosY(0f, slideDuration).SetEase(slideEase));
        currentSequence.Join(cg.DOFade(1f, slideDuration).SetEase(fadeEase));

        currentSequence.OnComplete(() =>
        {
            rt.anchoredPosition = Vector2.zero;
            cg.alpha = 1f;
            isAnimating = false;
            currentSequence = null;
        });

        currentSequence.OnKill(() =>
        {
            if (currentSequence != null)
                currentSequence = null;
        });
    }

    private void PlayFadeGroupAndShow(int[] fadeIndices, int showIndex)
    {
        if (!IsValidIndex(showIndex) || images[showIndex] == null)
            return;

        KillCurrentSequence();
        isAnimating = true;

        Image showImage = images[showIndex];
        RectTransform showRect = rects[showIndex];
        CanvasGroup showCG = canvasGroups[showIndex];

        showImage.gameObject.SetActive(true);
        showImage.transform.SetAsLastSibling();
        showRect.anchoredPosition = new Vector2(0f, startOffsetY);
        showCG.alpha = 0f;

        currentSequence = DOTween.Sequence().SetUpdate(true);

        if (fadeIndices != null)
        {
            for (int i = 0; i < fadeIndices.Length; i++)
            {
                int idx = fadeIndices[i];

                if (!IsValidIndex(idx))
                    continue;

                if (images[idx] == null || canvasGroups[idx] == null)
                    continue;

                if (!images[idx].gameObject.activeSelf)
                    continue;

                currentSequence.Join(
                    canvasGroups[idx].DOFade(0f, fadeDuration).SetEase(fadeEase)
                );
            }
        }

        currentSequence.Join(showRect.DOAnchorPosY(0f, slideDuration).SetEase(slideEase));
        currentSequence.Join(showCG.DOFade(1f, slideDuration).SetEase(fadeEase));

        currentSequence.OnComplete(() =>
        {
            if (fadeIndices != null)
            {
                for (int i = 0; i < fadeIndices.Length; i++)
                {
                    int idx = fadeIndices[i];

                    if (!IsValidIndex(idx))
                        continue;

                    if (images[idx] == null || canvasGroups[idx] == null)
                        continue;

                    canvasGroups[idx].alpha = 0f;
                    images[idx].gameObject.SetActive(false);
                }
            }

            showRect.anchoredPosition = Vector2.zero;
            showCG.alpha = 1f;

            isAnimating = false;
            currentSequence = null;
        });

        currentSequence.OnKill(() =>
        {
            if (currentSequence != null)
                currentSequence = null;
        });
    }

    private void PlayFadeAllAndDisable()
    {
        KillCurrentSequence();
        isAnimating = true;

        currentSequence = DOTween.Sequence().SetUpdate(true);

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null || canvasGroups[i] == null)
                continue;

            if (!images[i].gameObject.activeSelf)
                continue;

            currentSequence.Join(
                canvasGroups[i].DOFade(0f, fadeDuration).SetEase(fadeEase)
            );
        }

        currentSequence.OnComplete(() =>
        {
            HideAllImmediate();
            isAnimating = false;
            currentStep = SequenceStep.Finished;
            currentSequence = null;
            gameObject.SetActive(false);
        });

        currentSequence.OnKill(() =>
        {
            if (currentSequence != null)
                currentSequence = null;
        });
    }

    private void HideAllImmediate()
    {
        if (images == null)
            return;

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null)
                continue;

            if (rects != null && i < rects.Length && rects[i] != null)
            {
                rects[i].DOKill();
                rects[i].anchoredPosition = new Vector2(0f, startOffsetY);
            }

            if (canvasGroups != null && i < canvasGroups.Length && canvasGroups[i] != null)
            {
                canvasGroups[i].DOKill();
                canvasGroups[i].alpha = 0f;
            }

            images[i].gameObject.SetActive(false);
        }
    }

    private void KillCurrentSequence()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }

        if (rects != null)
        {
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i] != null)
                    rects[i].DOKill();
            }
        }

        if (canvasGroups != null)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
            {
                if (canvasGroups[i] != null)
                    canvasGroups[i].DOKill();
            }
        }
    }

    private bool IsValidIndex(int index)
    {
        return images != null && index >= 0 && index < images.Length;
    }
}