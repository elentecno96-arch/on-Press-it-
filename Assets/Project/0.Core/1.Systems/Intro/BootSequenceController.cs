using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Managers;
using UnityEngine;
using System.Collections;

namespace Project.Core.Systems.Intro
{
    /// <summary>
    /// 인트로 초기 화면 연출 및 실행 순서 컨트롤러
    /// </summary>
    public class BootSequenceController : MonoBehaviour
    {
        //페이드 지속 시간
        private const float FADE_IN_DURATION = 0.6f;
        private const float FADE_OUT_DURATION = 0.5f;
        private const float LOGO_FADE_IN_DURATION = 0.8f;

        //연출 사이 대기 시간
        private const float WARNING_WAIT_TIME = 1.5f;
        private const float LOGO_WAIT_TIME = 1.5f;

        //시작 텍스트 루프 시간
        private const float START_TEXT_LOOP_DURATION = 0.8f;

        [SerializeField] private CanvasGroup warningGroup; // Warning_Tx의 CanvasGroup
        [SerializeField] private CanvasGroup logoGroup;    // Logo_Image의 CanvasGroup
        [SerializeField] private CanvasGroup startGroup;   // Start_Tx의 CanvasGroup
        [SerializeField] private GameObject startObject;   // Start_Tx 오브젝트

        private void Start()
        {
            if (CheckComponents())
            {
                StartCoroutine(IntroSequenceRoutine());
            }
        }

        /// <summary>
        /// 널 방지 예외처리
        /// </summary>
        /// <returns></returns>
        private bool CheckComponents()
        {
            if (warningGroup == null || logoGroup == null || startGroup == null || startObject == null)
            {
                Debug.LogError("인트로의 부트스퀸스 컨트롤러의 인스펙터에 Ui가 할당되지 않았어용");
                return false;
            }
            return true;
        }

        private IEnumerator IntroSequenceRoutine()
        {
            warningGroup.alpha = 0f;
            logoGroup.alpha = 0f;
            startGroup.alpha = 0f;
            startObject.SetActive(false);

            yield return warningGroup.DOFade(1f, FADE_IN_DURATION).WaitForCompletion();

            yield return new WaitForSeconds(WARNING_WAIT_TIME);

            yield return warningGroup.DOFade(0f, FADE_OUT_DURATION).WaitForCompletion();

            yield return logoGroup.DOFade(1f, LOGO_FADE_IN_DURATION).SetEase(Ease.OutCubic).WaitForCompletion();

            yield return new WaitForSeconds(LOGO_WAIT_TIME);

            yield return logoGroup.DOFade(0f, FADE_OUT_DURATION).WaitForCompletion();

            startObject.SetActive(true);
            startGroup.DOFade(1f, START_TEXT_LOOP_DURATION).SetLoops(-1, LoopType.Yoyo).SetLink(startObject); //오브젝트가 파괴되면 트윈도 같이 kill

            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0)); //인풋 시스템으로 변경 예정

            EnterMainScene();
        }

        /// <summary>
        /// 게임 매니저를 통해 씬 전환 요청
        /// </summary>
        private void EnterMainScene()
        {
            Debug.Log("메인 씬으로 이동 요청");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnterGameScene("TestCore1").Forget(); //테스트 코드
                //GameManager.Instance.EnterGameScene("Main").Forget();
            }
            else
            {
                Debug.LogError("게임매니저가 존재 하지 않습니다 필드를 확인해주세요");
            }
        }
    }
}
