using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Stage3PlayerVisual : MonoBehaviour, ITouchVisual
{
    [SerializeField] private Image handImage;
    [SerializeField] private Sprite[] idleSprites;    
    [SerializeField] private Sprite[] holdSprites;    
    [SerializeField] private Sprite[] successSprites; 
    [SerializeField] private Sprite[] failSprites;    

    private float _animTimer;
    private int _animFrame;
    private bool _isHolding;
    private bool _isLocked;

    private void Update()
    {
        if (_isLocked) return;

        _animTimer += Time.deltaTime;
        if (_animTimer >= 0.15f)
        {
            _animTimer = 0f;
            _animFrame = (_animFrame + 1) % 2;

            if (_isHolding)
            {
                if (holdSprites.Length >= 2)
                    handImage.sprite = holdSprites[_animFrame];
            }
            else
            {
                if (idleSprites.Length >= 2)
                    handImage.sprite = idleSprites[_animFrame];
            }
        }
    }

    public void PlayAction(PatternType type)
    {
        if (_isLocked) return;

        if (type == PatternType.Hold)
        {
            _isHolding = true;
            _isLocked = false;
            _animFrame = 0;
        }
    }

    public void StopHoldAction()
    {
        _isHolding = false;
    }

    public void PlayAction(JudgeResult result)
    {
        StopAllCoroutines();
        StartCoroutine(JudgeResultRoutine(result));
    }

    private IEnumerator JudgeResultRoutine(JudgeResult result)
    {
        _isLocked = true;
        _isHolding = false;

        Sprite[] targetSprites = (result != JudgeResult.Miss) ? successSprites : failSprites;

        if (targetSprites != null && targetSprites.Length >= 2)
        {
            handImage.sprite = targetSprites[0];
            yield return new WaitForSeconds(0.1f);

            handImage.sprite = targetSprites[1];

            yield return new WaitForSeconds(0.5f);
        }
        else if (targetSprites != null && targetSprites.Length == 1)
        {
            handImage.sprite = targetSprites[0];
            yield return new WaitForSeconds(0.6f);
        }

        Unlock();
    }

    private void Unlock()
    {
        _isLocked = false;
        _animTimer = 0f;
        _animFrame = 0; 
    }

    public void UpdateVisual(float progress)
    {
    }
}
