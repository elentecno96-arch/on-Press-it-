using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public RectTransform logo;

    public RectTransform tapTextTransform;
    public TextMeshProUGUI tapText;

    public GameObject loadingBarBG;
    public Image loadingBar;

    bool canTap = false;

    void Start()
    {
        tapText.gameObject.SetActive(false);
        loadingBarBG.SetActive(false);

        PlayIntro();
    }

    void Update()
    {
        if (canTap && Input.GetMouseButtonDown(0))
        {
            StartLoading();
        }
    }

    void PlayIntro()
    {
        LogoAnimation();
    }

    void LogoAnimation()
    {
        logo.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            logo.DOScale(1.2f, 0.6f)
            .SetEase(Ease.OutBounce)
        );

        seq.Append(
            logo.DOScale(1f, 0.2f)
        );

        seq.AppendInterval(0.3f);

        seq.AppendCallback(() =>
        {
            ShowTapText();
        });
    }

    void ShowTapText()
    {
        tapText.gameObject.SetActive(true);

        tapTextTransform.anchoredPosition =
            new Vector2(-800, tapTextTransform.anchoredPosition.y);

        tapTextTransform.DOAnchorPosX(0, 0.8f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                TapIdle();
                canTap = true;
            });
    }

    void TapIdle()
    {
        tapText.DOFade(0.3f, 0.7f)
            .SetLoops(-1, LoopType.Yoyo);

        tapTextTransform.DORotate(new Vector3(0, 0, 3), 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void StartLoading()
    {
        canTap = false;

        tapText.DOKill();
        tapTextTransform.DOKill();

        tapText.gameObject.SetActive(false);

        loadingBarBG.SetActive(true);

        loadingBar.fillAmount = 0;

        loadingBar.DOFillAmount(1f, 3f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                LoadMainScene();
            });
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}