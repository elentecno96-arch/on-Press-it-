using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Project.Core.Managers
{
    // [설명] 스테이지별 인덱스와 최고 점수를 쌍으로 묶어 저장하는 데이터 클래스
    [System.Serializable]
    public class StageSaveData
    {
        public int stageIndex;
        public float bestScore;
    }

    // [설명] 업적의 ID, 이름, 해금 상태 및 날짜를 저장하는 데이터 클래스
    [System.Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        public bool isUnlocked;
        public string unlockDate;
    }

    // [설명] 파일로 저장될 최종 루트 데이터 객체
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
        // 이유: 플랫폼마다 다른 권한이 있는 안전한 저장 경로를 반환함
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

        public async UniTask SyncWithServer()
        {
            try
            {
                if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized) return;

                var snapshot = await FirebaseManager.Instance.DbRef
                    .Child("users").Child(FirebaseManager.Instance.User.UserId).Child("playerData")
                    .GetValueAsync().AsUniTask();

                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    PlayerData serverData = JsonUtility.FromJson<PlayerData>(json);

                    Data = serverData; //덥어쓰기

                    SaveLocal();
                    Debug.Log("[PlayerManager] 서버 데이터 동기화 완료!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerManager] 서버 동기화 중 에러: {e.Message}");
            }
        }

        // AudioManager로부터 전달받은 볼륨 정보를 데이터에 반영하고 저장
        private void UpdateAudioSettings(float bgm, float sfx)
        {
            Data.bgmVolume = bgm;
            Data.sfxVolume = sfx;

            Save();
            Debug.Log($"[PlayerManager] 오디오 설정 저장 완료: BGM({bgm}), SFX({sfx})");
        }

        // 설명: 특정 스테이지의 점수가 이전보다 높을 때만 갱신 후 저장
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
        // 특정 스테이지가 클리어되었는지 확인하는 헬퍼 메서드 ---
        public bool IsStageCleared(int stageIndex)
        {
            // [수정] 0 이하는 기록이 존재할 수 없으므로 무조건 false입니다.
            // 첫 스테이지 오픈은 Presenter의 (myStageNum == 1) 로직이 담당합니다.
            if (stageIndex <= 0) return false;

            if (Data == null || Data.stageRecords == null) return false;

            var record = Data.stageRecords.Find(s => s.stageIndex == stageIndex);

            if (record != null)
            {
                Debug.Log($"[데이터확인] 인덱스:{stageIndex} / 점수:{record.bestScore}");
                // 점수가 0보다 커야 클리어로 인정
                return record.bestScore > 0;
            }

            // 기록이 아예 없으면 당연히 false
            return false;
        }

        // 설명: JSON 형식으로 데이터를 직렬화하여 실제 파일에 작성
        public void Save()
        {
            SaveLocal();

            if (FirebaseManager.Instance != null)
            {
                string json = JsonUtility.ToJson(Data);
                string path = $"users/{FirebaseManager.Instance.User.UserId}/playerData";

                FirebaseManager.Instance.SaveDataWithCheck(path, json);
            }
        }

        private void SaveLocal()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e) { Debug.LogError($"[Local Save 실패] {e.Message}"); }
        }

        // 설명: 파일이 존재하면 읽어와서 JsonUtility를 통해 C# 객체로 변환
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
        // 이유: 오브젝트 파괴 시 이벤트 연결을 해제하여 메모리 누수 및 에러 방지
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