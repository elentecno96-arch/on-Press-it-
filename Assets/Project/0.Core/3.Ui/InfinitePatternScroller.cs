using UnityEngine;

public class AdvancedPatternScroller : MonoBehaviour
{
    [Header("패턴 세트")]
    [SerializeField] private RectTransform setA;
    [SerializeField] private RectTransform setB;

    [Header("패턴 리스트")]
    [SerializeField] private RectTransform[] patternAList;
    [SerializeField] private RectTransform[] patternBList;

    [Header("이동 설정")]
    [SerializeField] private float setWidth = 1400f;
    [SerializeField] private float setSpeed = 25f;

    [Header("개별 이동")]
    [SerializeField] private float patternASpeed = 10f;
    [SerializeField] private float patternBSpeed = 5f;

    [Header("이동 설정")]
    [SerializeField] private bool moveLeft = true;

    private void Start()
    {
        setA.anchoredPosition = new Vector2(0f, setA.anchoredPosition.y);
        setB.anchoredPosition = new Vector2(setWidth, setB.anchoredPosition.y);
    }

    private void Update()
    {
        float dir = moveLeft ? -1f : 1f;

        float setMove = setSpeed * dir * Time.deltaTime;
        float aMove = patternASpeed * dir * Time.deltaTime;
        float bMove = patternBSpeed * dir * Time.deltaTime;

        // 세트 이동
        setA.anchoredPosition += new Vector2(setMove, 0f);
        setB.anchoredPosition += new Vector2(setMove, 0f);

        // 개별 이동
        foreach (var a in patternAList)
        {
            a.anchoredPosition += new Vector2(aMove, 0f);
        }

        foreach (var b in patternBList)
        {
            b.anchoredPosition += new Vector2(bMove, 0f);
        }

        // 세트 반복
        if (moveLeft)
        {
            if (setA.anchoredPosition.x <= -setWidth)
            {
                setA.anchoredPosition = new Vector2(
                    setB.anchoredPosition.x + setWidth,
                    setA.anchoredPosition.y
                );
            }

            if (setB.anchoredPosition.x <= -setWidth)
            {
                setB.anchoredPosition = new Vector2(
                    setA.anchoredPosition.x + setWidth,
                    setB.anchoredPosition.y
                );
            }
        }
        else
        {
            if (setA.anchoredPosition.x >= setWidth)
            {
                setA.anchoredPosition = new Vector2(
                    setB.anchoredPosition.x - setWidth,
                    setA.anchoredPosition.y
                );
            }

            if (setB.anchoredPosition.x >= setWidth)
            {
                setB.anchoredPosition = new Vector2(
                    setA.anchoredPosition.x - setWidth,
                    setB.anchoredPosition.y
                );
            }
        }
    }
}