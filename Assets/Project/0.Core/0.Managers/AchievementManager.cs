using UnityEngine;
using System;
using System.Collections.Generic;
using Project.Core.Utilities;
using Project.Rhythm.Data;
using Cysharp.Threading.Tasks;

namespace Project.Core.Managers
{
    public class AchievementManager : BaseSingleton<AchievementManager>
    {
        public override async UniTask Initialize()
        {
            if (IsInitialized) return;
            await UniTask.Yield();
            IsInitialized = true;
        }

        public void CheckStageAchievements(StageData data, int perfectCount, bool isFirstClear)
        {
            int index = data.stageIndex;
            int totalNotes = data.actions.Count;

            // 1. 최초 클리어
            if (isFirstClear)
            {
                Unlock($"Clear_{index}", $"{data.stageName} 최초 클리어!");
            }

            // 2. All Perfect
            if (perfectCount >= totalNotes && totalNotes > 0)
            {
                Unlock($"AllPerfect_{index}", $"{data.stageName} ALL PERFECT!");
            }

            // 3. 보스 스테이지 클리어
            if (data.stageName.Contains("Boss") || index == 99)
            {
                Unlock($"BossClear_{index}", $"보스 정복자: {data.stageName}");
            }

            PlayerManager.Instance.Save();
        }

        private void Unlock(string id, string title)
        {
            var list = PlayerManager.Instance.Data.achievements;
            if (list.Exists(a => a.id == id)) return;

            list.Add(new AchievementData
            {
                id = id,
                title = title,
                isUnlocked = true,
                unlockDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            Debug.Log($"<color=cyan><b>[업적 달성]</b></color> {title}");
        }
    }
}