using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using Project.Rhythm.Data;
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
        public List<StageSaveData> stageRecords = new();
        public List<AchievementData> achievements = new();

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

            // OnStageClearRequested(점수포함) -> OnStagePlayCompleted(인덱스만)로 변경 대응
            StageData.OnStagePlayStatusChanged += HandleStageStatusChanged;

            await UniTask.Yield();
            IsInitialized = true;
        }

        // 점수와 상관없이 "플레이 했다"는 기록 자체를 남기는 메서드
        private void HandleStageStatusChanged(int index, bool isPlayed)
        {
            // False(플레이 안 함/실패 등)라면 아무것도 하지 않고 리턴합니다.
            if (!isPlayed || index <= 0) return;

            var record = Data.stageRecords.Find(s => s.stageIndex == index);

            // 기록이 아예 없는 경우에만 '플레이 함' 상태를 위해 데이터 생성 (기본 점수 0)
            if (record == null)
            {
                record = new StageSaveData { stageIndex = index, bestScore = 0 };
                Data.stageRecords.Add(record);

                Save(); // 새로운 스테이지 플레이(True) 시 즉시 저장
                Debug.Log($"[PlayerManager] {index}번 스테이지가 '플레이 됨(True)' 상태로 기록되었습니다.");
            }
        }
        public bool IsStageCleared(int stageIndex)
        {
            if (stageIndex <= 0) return false;
            if (Data == null || Data.stageRecords == null) return false;

            // [변경] 점수가 0보다 큰지가 아니라, 리스트에 해당 스테이지 인덱스가 존재하는지로 판단
            // 즉, 한 번이라도 완주해서 HandleStageCompleted가 실행되었다면 true입니다.
            bool hasPlayed = Data.stageRecords.Exists(s => s.stageIndex == stageIndex);

            return hasPlayed;
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

        //StageData에서 호출할 범용 저장 메서드입니다.
        public void SaveStageResult(int index, float score)
        {
            if (index <= 0) return;

            var record = Data.stageRecords.Find(s => s.stageIndex == index);
            if (record == null)
            {
                record = new StageSaveData { stageIndex = index, bestScore = score };
                Data.stageRecords.Add(record);
            }
            else if (score > record.bestScore)
            {
                record.bestScore = score;
            }
            else
            {
                // 점수가 더 높지 않으면 저장하지 않고 리턴
                return;
            }

            Save(); // 로컬 및 서버 저장 통합 함수 호출
            Debug.Log($"[PlayerManager] 스테이지 {index} 결과 저장 완료: {score}점");
        }
        //StageData의 BestScore 프로퍼티에서 호출할 헬퍼 메서드입니다.
        public int GetBestScore(int index)
        {
            var record = Data.stageRecords.Find(s => s.stageIndex == index);
            return record != null ? (int)record.bestScore : 0;
        }

        // 설명: 특정 스테이지의 점수가 이전보다 높을 때만 갱신 후 저장
        public void SaveBestScore(int index, float score)
        {
            //SaveStageResult와 동일한 로직이므로 내부에서 호출하도록 변경 가능
            SaveStageResult(index, score);
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnRequestAudioSave -= UpdateAudioSettings;
            }
            // [수정] 이벤트 해제 구문 변경
            StageData.OnStagePlayStatusChanged -= HandleStageStatusChanged;
        }
    }
}