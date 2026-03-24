using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Managers
{
    [System.Serializable]
    public class StageSaveData
    {
        public int stageIndex;
        public float bestScore;
    }

    [System.Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        public bool isUnlocked;
        public string unlockDate;
    }

    [System.Serializable]
    public class PlayerData
    {
        public List<StageSaveData> stageRecords = new List<StageSaveData>();
        public List<AchievementData> achievements = new List<AchievementData>();
    }

    public class PlayerManager : BaseSingleton<PlayerManager>
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "PlayerSave.json");
        public PlayerData Data { get; private set; } = new PlayerData();

        public override async UniTask Initialize()
        {
            if (IsInitialized) return;
            Load();
            await UniTask.Yield();
            IsInitialized = true;
        }

        public void SaveBestScore(int index, float score)
        {
            var record = Data.stageRecords.Find(s => s.stageIndex == index);
            if (record == null)
            {
                Data.stageRecords.Add(new StageSaveData { stageIndex = index, bestScore = score });
            }
            else if (score > record.bestScore)
            {
                record.bestScore = score;
            }
            else return;

            Save();
            Debug.Log($"[저장완료] 스테이지 {index} : {score}");
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e) { Debug.LogError($"[Save 실패] {e.Message}"); }
        }

        private void Load()
        {
            if (!File.Exists(SavePath)) return;
            try
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<PlayerData>(json);
            }
            catch { Data = new PlayerData(); }
        }
    }
}