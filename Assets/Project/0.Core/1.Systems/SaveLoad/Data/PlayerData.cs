using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Systems.SaveLoad.Data
{
    /// <summary>
    /// 플레이어 데이터 저장 래퍼 클래스
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public string userName = "New Player"; //유저 이름 기본값
        public string userId = "";

        public List<StageSaveData> stageRecords = new();
        public List<DetailedStageRecord> detailedRecords = new(); //각 스테이지 기록을 담는 리스트
        public List<AchievementData> achievements = new();

        // 오디오 설정
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;

        /// <summary>
        /// 외부(서버) 데이터와 현재 데이터를 병합합니다. (더 나은 기록 유지)
        /// </summary>
        public void Merge(PlayerData server)
        {
            if (server == null) return;

            // 점수 병합
            foreach (var serverRecord in server.stageRecords)
            {
                var local = stageRecords.Find(s => s.stageIndex == serverRecord.stageIndex);
                if (local == null) stageRecords.Add(serverRecord);
                else local.bestScore = Mathf.Max(local.bestScore, serverRecord.bestScore);
            }

            // 업적 병합
            foreach (var serverAch in server.achievements)
            {
                var local = achievements.Find(a => a.id == serverAch.id);
                if (local == null) achievements.Add(serverAch);
                else if (serverAch.isUnlocked && !local.isUnlocked)
                {
                    local.isUnlocked = true;
                    local.unlockDate = serverAch.unlockDate;
                }
            }

            // 설정값은 서버 우선 (또는 선택)
            bgmVolume = server.bgmVolume;
            sfxVolume = server.sfxVolume;
        }
    }
}
