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

        // 오디오 설정을 저장하기 위한 변수 추가 (기본값 1.0f)
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;
    }

    public class PlayerManager : BaseSingleton<PlayerManager>
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "PlayerSave.json");
        public PlayerData Data { get; private set; } = new PlayerData();

        public override async UniTask Initialize()
        {
            if (IsInitialized) return;
            Load();

            // AudioManager의 이벤트를 구독하여 데이터 수신 대기
            // AudioManager.Instance가 존재할 때만 이벤트를 연결합니다.
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnRequestAudioSave += UpdateAudioSettings;
            }

            await UniTask.Yield();
            IsInitialized = true;
        }
        // AudioManager로부터 전달받은 볼륨 정보를 데이터에 반영하고 저장
        private void UpdateAudioSettings(float bgm, float sfx)
        {
            Data.bgmVolume = bgm;
            Data.sfxVolume = sfx;

            Save();
            Debug.Log($"[PlayerManager] 오디오 설정 저장 완료: BGM({bgm}), SFX({sfx})");
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
        // 메모리 누수 방지를 위해 오브젝트 파괴 시 이벤트 구독 해제
        private void OnDisable()
        {
            // 싱글톤 인스턴스가 존재할 때만 구독 해제
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnRequestAudioSave -= UpdateAudioSettings;
            }
        }
    }
}