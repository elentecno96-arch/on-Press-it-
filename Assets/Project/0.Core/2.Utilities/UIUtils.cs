using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Core.Utilities
{
    public static class UIUtils
    {
        /// <summary>
        /// 특정 스크린 좌표가 UI 요소 위에 있는지 확인합니다.
        /// </summary>
        public static bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("[UIUtils] 씬에 EventSystem이 없습니다!");
                return false;
            }

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        /// <summary>
        /// (디버깅용) 특정 좌표에 걸린 UI 오브젝트 리스트를 반환합니다.
        /// </summary>
        public static List<RaycastResult> GetUIHitResults(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return new List<RaycastResult>();

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results;
        }
    }
}
