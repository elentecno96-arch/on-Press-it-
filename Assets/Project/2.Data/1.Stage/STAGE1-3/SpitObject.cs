using UnityEngine;
using UnityEngine.UI;

public class SpitObject : MonoBehaviour
{
    [SerializeField] private Sprite[] animSprites;      // 반짝이거나 떠는 이미지 3장
    [SerializeField] private float frameRate = 0.1f;    // 프레임 전환 속도 (초)

    [SerializeField] private float lifeTime = 2.0f;     // 생성 후 자동 삭제 시간 (초)

    private Image _image;
    private float _animTimer;
    private int _animFrame;

    private void Awake()
    {
        _image = GetComponent<Image>();

        if (animSprites == null || animSprites.Length < 3)
        {
            Debug.LogWarning($"{gameObject.name}의 SpitObject 애니메이션 이미지가 3장 미만입니다.");
        }
    }

    private void OnEnable()
    {
        _animTimer = 0f;
        _animFrame = 0;
        if (animSprites != null && animSprites.Length > 0)
        {
            _image.sprite = animSprites[0];
        }

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (animSprites == null || animSprites.Length < 3) return;

        _animTimer += Time.deltaTime;
        if (_animTimer >= frameRate)
        {
            _animTimer = 0f;
            _animFrame = (_animFrame + 1) % animSprites.Length;
            _image.sprite = animSprites[_animFrame];
        }
    }
}
