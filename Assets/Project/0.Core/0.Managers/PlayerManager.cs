using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Core.Managers
{
    [System.Serializable]
    public class StageSaveData
    {
        public int stageIndex;
        public float bestScore; // 최고 판정 점수
    }
    [System.Serializable]
    public class PlayerData
    {
        public List<StageSaveData> stageRecords = new List<StageSaveData>();
    }
    public class PlayerManager : BaseSingleton<PlayerManager>
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "PlayerSave.json");
        public PlayerData Data { get; private set; } = new PlayerData();

        public override async UniTask Initialize()
        {
            if (IsInitialized) return;
            Load(); // 초기화 시 데이터 로드
            await UniTask.Yield();
            IsInitialized = true;
        }
        // 최고 점수 갱신 및 저장
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
            else return; // 기존 점수가 더 높으면 무시

            File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
            Debug.Log($"[저장완료] 스테이지 {index} : {score}");
        }
        // 콘솔에 기록 출력
        public void PrintRecord(int index)
        {
            var record = Data.stageRecords.Find(s => s.stageIndex == index);
            string result = record != null ? record.bestScore.ToString() : "기록 없음";
            Debug.Log($"<color=yellow>[최고기록]</color> 스테이지 {index} : {result}");
        }
        /// <summary>
        /// 특정 스테이지의 기록을 갱신하고 저장합니다. (기존보다 높을 때만)
        /// </summary>
        public void UpdateStageRecord(int stageIndex, float newScore)
        {
            var record = Data.stageRecords.Find(s => s.stageIndex == stageIndex);

            if (record == null)
            {
                Data.stageRecords.Add(new StageSaveData { stageIndex = stageIndex, bestScore = newScore });
            }
            else
            {
                // [중요] 기존 점수보다 높을 때만 갱신
                if (newScore > record.bestScore)
                {
                    record.bestScore = newScore;
                }
                else
                {
                    return; // 갱신 불필요 시 저장 생략
                }
            }
            Save();
        }
        /// <summary>
        /// 특정 스테이지의 기록을 콘솔에 출력합니다.
        /// </summary>
        public void PrintStageRecord(int stageIndex)
        {
            var record = Data.stageRecords.Find(s => s.stageIndex == stageIndex);
            if (record != null)
            {
                Debug.Log($"<color=white>[PlayerManager]</color> <b>스테이지 {stageIndex}</b> 최고 기록: <color=yellow>{record.bestScore}</color>");
            }
            else
            {
                Debug.Log($"<color=orange>[PlayerManager]</color> 스테이지 {stageIndex}의 기록이 존재하지 않습니다.");
            }
        }
        private void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[PlayerManager] 저장 완료: {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerManager] 저장 실패: {e.Message}");
            }
        }
        private void Load()
        {
            if (!File.Exists(SavePath))
            {
                Data = new PlayerData();
                return;
            }
            try
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<PlayerData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerManager] 로드 실패: {e.Message}");
                Data = new PlayerData();
            }
        }
    }
}