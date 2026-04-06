using Project.UI.Profile.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.View
{
    /// <summary>
    /// 프로필 추상 뷰
    /// </summary>
    public abstract class BaseProfileView : MonoBehaviour
    {
        [SerializeField] protected Button toMainBtn;
        [SerializeField] protected Button toMissionBtn;
        [SerializeField] protected Button toHelpBtn;
        [SerializeField] protected Button closeBtn;

        public Action<ProfileTabType> OnTabRequest;
        public Action OnCloseRequest;

        public virtual void Init()
        {
            toMainBtn?.onClick.AddListener(() => OnTabRequest?.Invoke(ProfileTabType.Main));
            toMissionBtn?.onClick.AddListener(() => OnTabRequest?.Invoke(ProfileTabType.Mission));
            toHelpBtn?.onClick.AddListener(() => OnTabRequest?.Invoke(ProfileTabType.Help));
            closeBtn?.onClick.AddListener(() => OnCloseRequest?.Invoke());
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);
    }
}
