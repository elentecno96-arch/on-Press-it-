using Project.Core.Managers;
using Project.UI.Profile.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Profile.View
{
    /// <summary>
    /// 프로필 미션 탭 뷰
    /// </summary>
    public class ProfileMissionView : BaseProfileView
    {
        [SerializeField] private Transform contentParent;           
        [SerializeField] private AchievementItem itemPrefab;        

        public override void Init()
        {
            base.Init();
            RefreshAchievementList();
        }

        public void RefreshAchievementList()
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            var myAchievements = PlayerManager.Instance.Data.achievements;

            if (myAchievements == null || myAchievements.Count == 0) return;

            foreach (var data in myAchievements)
            {
                if (data.isUnlocked)
                {
                    var item = Instantiate(itemPrefab, contentParent);
                    item.Setup(data.title, data.unlockDate);
                }
            }
        }
    }
}
