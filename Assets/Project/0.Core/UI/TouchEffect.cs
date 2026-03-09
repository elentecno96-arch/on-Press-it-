using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEffect : MonoBehaviour
{
    public GameObject effectPrefab; //이펙트 프리팹
    public float effectScale = 0.3f; //이펙트 크기
    public float trailDistance = 0.5f; //이펙트 생성 거리

    private GameObject currentEffect;
    private Vector3 lastSpawnPos;

    void Update()
    {
        //시작
        if (Input.GetMouseButtonDown(0))
        {
            SpawnEffect(Input.mousePosition);
        }

        //유지
        if (Input.GetMouseButton(0) && currentEffect != null)
        {
            Vector3 worldPos = ScreenToWorld(Input.mousePosition);
            currentEffect.transform.position = worldPos;

            if (Vector3.Distance(worldPos, lastSpawnPos) > trailDistance)
            {
                SpawnEffect(Input.mousePosition);
            }
        }

        //종료
        if (Input.GetMouseButtonUp(0) && currentEffect != null)
        {
            ParticleSystem ps = currentEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();

            currentEffect = null;
        }
    }

    void SpawnEffect(Vector3 screenPos)
    {
        Vector3 worldPos = ScreenToWorld(screenPos);

        currentEffect = Instantiate(effectPrefab, worldPos, Quaternion.identity);
        currentEffect.transform.localScale = Vector3.one * effectScale;

        lastSpawnPos = worldPos;
    }

    Vector3 ScreenToWorld(Vector3 screenPos)
    {
        screenPos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
