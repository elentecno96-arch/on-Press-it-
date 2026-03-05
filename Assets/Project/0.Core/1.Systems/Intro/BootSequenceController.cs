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
        [SerializeField] private CanvasGroup warningGroup; // Warning_Tx의 CanvasGroup
        [SerializeField] private CanvasGroup logoGroup;    // Logo_Image의 CanvasGroup
        [SerializeField] private CanvasGroup startGroup;   // Start_Tx의 CanvasGroup
        [SerializeField] private GameObject startObject;   // Start_Tx 오브젝트

        private void Start()
        {
            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            warningGroup.alpha = 0f;
            logoGroup.alpha = 0f;
            startGroup.alpha = 0f;
            startObject.SetActive(false);

            yield return warningGroup.DOFade(1f, 0.6f).WaitForCompletion();

            yield return new WaitForSeconds(1.5f);

            yield return warningGroup.DOFade(0f, 0.5f).WaitForCompletion();

            yield return logoGroup.DOFade(1f, 0.8f).SetEase(Ease.OutCubic).WaitForCompletion();

            yield return new WaitForSeconds(1.5f);

            yield return logoGroup.DOFade(0f, 0.5f).WaitForCompletion();

            startObject.SetActive(true);
            startGroup.DOFade(1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetLink(startObject);

            Debug.Log("Waiting for Touch...");
            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));

            EnterMainScene();
        }

        /// <summary>
        /// 게임 매니저를 통해 씬 전환 요청
        /// </summary>
        private void EnterMainScene()
        {
            GameManager.Instance.EnterGameScene("TestCore1").Forget();
        }
    }
}
