using Project.Core.Systems.SaveLoad.Data;
using System;

namespace Project.Core.Systems.SaveLoad
{
    /// <summary>
    /// 플레이어 데이터 기록
    /// </summary>
    public static class DataRecord
    {
        // 스테이지 클리어 여부 계산
        public static bool IsCleared(PlayerData data, int index, float threshold)
        {
            if (index <= 0 || data?.stageRecords == null) return false;
            var record = data.stageRecords.Find(s => s.stageIndex == index);
            return record != null && record.bestScore >= threshold;
        }

        // 결과 반영 및 최고 기록 갱신 여부 반환
        public static bool UpdateStageResult(PlayerData data, int index, float score)
        {
            var record = data.stageRecords.Find(s => s.stageIndex == index);
            bool isBestUpdated = false;

            if (record == null)
            {
                data.stageRecords.Add(new StageSaveData { stageIndex = index, bestScore = score });
                isBestUpdated = true;
            }
            else if (score > record.bestScore)
            {
                record.bestScore = score;
                isBestUpdated = true;
            }

            UpdateDetailed(data, index, score);
            return isBestUpdated;
        }

        // 상세 기록(Top 3) 갱신
        private static void UpdateDetailed(PlayerData data, int index, float score)
        {
            var detailed = data.detailedRecords.Find(d => d.stageIndex == index);
            if (detailed == null)
            {
                detailed = new DetailedStageRecord { stageIndex = index };
                data.detailedRecords.Add(detailed);
            }

            detailed.records.Add(new ScoreRecord { score = score, date = DateTime.Now.ToString("yyyy-MM-dd") });
            detailed.records.Sort((a, b) => b.score.CompareTo(a.score));

            if (detailed.records.Count > 3)
                detailed.records.RemoveRange(3, detailed.records.Count - 3);
        }

        /// <summary>
        /// 특정 스테이지의 최고 점수를 반환합니다.
        /// </summary>
        public static float GetBestScore(PlayerData data, int index)
        {
            if (index <= 0 || data?.stageRecords == null) return 0f;

            var record = data.stageRecords.Find(s => s.stageIndex == index);
            return record != null ? record.bestScore : 0f;
        }
    }
}
