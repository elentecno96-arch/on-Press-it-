using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEffect : MonoBehaviour
{
    public GameObject effectPrefab; //이펙트 프리팹
    public float effectScale = 0.3f; //이펙트 크기
    public float trailDistance = 0.5f; //이펙트 생성 거리

    public RectTransform touchArea;

    private GameObject currentEffect;
    private Vector3 lastSpawnPos;

    void Update()
    {
        //터치/마우스 좌표 통합
        Vector3 inputPos = GetInputPosition();

        //입력이 없으면 종료
        if (inputPos == Vector3.zero) return;

        //터치 영역 체크
        if (!IsInsideTouchArea(inputPos)) return;

        //시작
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SpawnEffect(inputPos);
        }

        //유지
        if ((Input.GetMouseButton(0) || Input.touchCount > 0) && currentEffect != null)
        {
            Vector3 worldPos = ScreenToWorld(inputPos);
            currentEffect.transform.position = worldPos;

            if (Vector3.Distance(worldPos, lastSpawnPos) > trailDistance)
            {
                SpawnEffect(inputPos);
            }
        }

        //종료
        if (Input.GetMouseButtonUp(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if (currentEffect != null)
            {
                ParticleSystem ps = currentEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Stop();

                currentEffect = null;
            }
        }
    }

    void SpawnEffect(Vector3 screenPos)
    {
        Vector3 worldPos = ScreenToWorld(screenPos);

        currentEffect = Instantiate(effectPrefab, worldPos, Quaternion.identity);
        currentEffect.transform.localScale = Vector3.one * effectScale;

        Destroy(currentEffect, 5f);

        lastSpawnPos = worldPos;
    }

    Vector3 ScreenToWorld(Vector3 screenPos)
    {
        screenPos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    //마우스와 터치 입력 좌표 통합
    Vector3 GetInputPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            return Input.mousePosition;
        }

        return Vector3.zero;
    }

    //터치가 TouchArea 안에 있는지 검사
    bool IsInsideTouchArea(Vector3 screenPos)
    {
        if (touchArea == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            touchArea,
            screenPos,
            null
        );
    }
}
