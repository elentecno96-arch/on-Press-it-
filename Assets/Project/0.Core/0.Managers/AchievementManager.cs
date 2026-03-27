using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using Project.Rhythm.Data;
using Project.Rhythm.Note;
using Project.Rhythm.Data.Enum;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
            if (data == null) return;

            int index = data.stageIndex;
            int totalNotes = 0;

            if(data.actions != null)
            {
                foreach (var action in data.actions)
                {
                    if(action.noteType == NoteType.Signal)
                    {
                        continue;
                    }
                    totalNotes++;
                }
            }

            // 디버깅용: 현재 체크 중인 값 확인
            Debug.Log($"[업적 체크] 스테이지: {data.stageName}, Perfect: {perfectCount}/{totalNotes}, 최초클리어: {isFirstClear}");

            // 1. 최초 클리어
            if (isFirstClear)
            {
                Unlock($"Clear_{index}", $"{data.stageName} 최초 클리어!");
            }

            // 2. All Perfect
            if (totalNotes > 0 && perfectCount >= totalNotes)
            {
                Unlock($"AllPerfect_{index}", $"{data.stageName} ALL PERFECT!");
            }

            // 3. 보스 스테이지 클리어
            // 대소문자 무관하게 "Boss" 포함 여부 확인
            bool isBossName = data.stageName.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isBossName || index == 99)
            {
                Unlock($"BossClear_{index}", $"보스 정복자: {data.stageName}");
            }

            PlayerManager.Instance.Save();
        }
        private void Unlock(string id, string title)
        {
            if (PlayerManager.Instance == null || PlayerManager.Instance.Data == null) return;

            var list = PlayerManager.Instance.Data.achievements;

            // 이미 달성한 업적이면 스킵
            if (list.Exists(a => a.id == id)) return;

            list.Add(new AchievementData
            {
                id = id,
                title = title,
                isUnlocked = true,
                unlockDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            // 업적 달성 시 하늘색 로그로 강조
            Debug.Log($"<color=cyan><b>[업적 달성!]</b></color> <color=cyan>{title}</color>");
        }
    }
}