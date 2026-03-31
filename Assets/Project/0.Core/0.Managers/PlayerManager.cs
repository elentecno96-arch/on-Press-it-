using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using Project.Rhythm.Data;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Project.Core.Managers
{
    // 스테이지별 최고 점수 데이터
    [System.Serializable]
    public class StageSaveData
    {
        public int stageIndex;
        public float bestScore;
    }

    // 업적 데이터 (해금 여부 포함)
    [System.Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        public bool isUnlocked;
        public string unlockDate;
    }

    // 플레이어 전체 저장 데이터
    [System.Serializable]
    public class PlayerData
    {
        public List<StageSaveData> stageRecords = new();
        public List<AchievementData> achievements = new();

        // 오디오 설정
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;
    }

    public class PlayerManager : BaseSingleton<PlayerManager>
    {
        // 서버 저장 호출 쿨타임 (Firebase write 남용 방지)
        private float lastSaveTime;
        private const float SAVE_COOLDOWN = 0.5f;

        // 플랫폼별 안전한 로컬 저장 경로
        private string SavePath => Path.Combine(Application.persistentDataPath, "PlayerSave.json");

        public PlayerData Data { get; private set; } = new PlayerData();

        public override async UniTask Initialize()
        {
            if (IsInitialized) return;

            // 로컬 데이터 먼저 로드 (오프라인 대비)
            Load();

            // 오디오 설정 변경 이벤트 구독
            if (AudioManager.Instance != null)
                AudioManager.Instance.OnRequestAudioSave += UpdateAudioSettings;

            // 스테이지 플레이 기록 이벤트
            StageData.OnStagePlayStatusChanged += HandleStageStatusChanged;

            await UniTask.Yield();
            IsInitialized = true;
        }

        // 스테이지를 "플레이한 적 있음" 기록
        private void HandleStageStatusChanged(int index, bool isPlayed)
        {
            if (!isPlayed || index <= 0) return;

            var record = Data.stageRecords.Find(s => s.stageIndex == index);

            // 최초 플레이 시에만 기록 생성
            if (record == null)
            {
                record = new StageSaveData { stageIndex = index, bestScore = 0 };
                Data.stageRecords.Add(record);

                Save();
                Debug.Log($"[PlayerManager] {index}번 스테이지 플레이 기록 생성");
            }
        }

        // 스테이지 플레이 여부 체크
        public bool IsStageCleared(int stageIndex)
        {
            if (stageIndex <= 0 || Data?.stageRecords == null) return false;
            return Data.stageRecords.Exists(s => s.stageIndex == stageIndex);
        }

        // 서버 데이터 동기화 (Merge 방식)
        public async UniTask SyncWithServer()
        {
            try
            {
                // Firebase 초기화 안됐으면 중단
                if (FirebaseManager.Instance == null ||
                    !FirebaseManager.Instance.IsInitialized)
                    return;

                string json = await FirebaseManager.Instance.LoadPlayerData();

                if (!string.IsNullOrEmpty(json))
                {
                    PlayerData serverData = JsonUtility.FromJson<PlayerData>(json);

                    // 서버 데이터를 로컬과 병합 (덮어쓰기 방지)
                    MergeData(serverData);

                    // 병합 결과를 로컬에 반영
                    SaveLocal();

                    Debug.Log("[PlayerManager] 서버 데이터 Merge 완료");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerManager] 서버 동기화 에러: {e.Message}");
            }
        }

        // 서버 데이터와 로컬 데이터 병합
        private void MergeData(PlayerData server)
        {
            if (server == null) return;

            // ===== 점수 병합 =====
            // 더 높은 점수를 우선 유지
            if (server.stageRecords != null)
            {
                foreach (var serverRecord in server.stageRecords)
                {
                    var local = Data.stageRecords.Find(s => s.stageIndex == serverRecord.stageIndex);

                    if (local == null)
                        Data.stageRecords.Add(serverRecord);
                    else
                        local.bestScore = Mathf.Max(local.bestScore, serverRecord.bestScore);
                }
            }

            // ===== 업적 병합 =====
            // 해금 상태를 우선 반영
            if (server.achievements != null)
            {
                foreach (var serverAch in server.achievements)
                {
                    var local = Data.achievements.Find(a => a.id == serverAch.id);

                    if (local == null)
                    {
                        Data.achievements.Add(serverAch);
                    }
                    else if (serverAch.isUnlocked)
                    {
                        local.isUnlocked = true;
                        local.unlockDate = serverAch.unlockDate;
                    }
                }
            }

            // ===== 설정값 =====
            // 설정은 서버 기준으로 덮어써도 문제 없음
            Data.bgmVolume = server.bgmVolume;
            Data.sfxVolume = server.sfxVolume;
        }

        // 오디오 설정 업데이트 시 저장
        private void UpdateAudioSettings(float bgm, float sfx)
        {
            Data.bgmVolume = bgm;
            Data.sfxVolume = sfx;

            Save();
        }

        // 스테이지 점수 저장 (최고 점수만 유지)
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
                // 기존 점수가 더 높으면 저장 안함
                return;
            }

            Save();
            Debug.Log($"[PlayerManager] 스테이지 {index} 점수 저장: {score}");
        }

        public int GetBestScore(int index)
        {
            var record = Data.stageRecords.Find(s => s.stageIndex == index);
            return record != null ? (int)record.bestScore : 0;
        }

        public void SaveBestScore(int index, float score)
        {
            SaveStageResult(index, score);
        }

        // 로컬 + 서버 저장
        public void Save()
        {
            // 로컬은 항상 저장 (데이터 유실 방지)
            SaveLocal();

            // 서버 저장 쿨타임 체크 (과도한 write 방지)
            if (Time.time - lastSaveTime < SAVE_COOLDOWN)
                return;

            lastSaveTime = Time.time;

            // Firebase 준비 안됐으면 종료
            if (FirebaseManager.Instance == null ||
                !FirebaseManager.Instance.IsInitialized)
                return;

            string json = JsonUtility.ToJson(Data);
            FirebaseManager.Instance.SavePlayerData(json);
        }

        // 로컬 파일 저장
        private void SaveLocal()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Local Save 실패] {e.Message}");
            }
        }

        // 로컬 파일 로드
        private void Load()
        {
            if (!File.Exists(SavePath)) return;

            try
            {
                string json = File.ReadAllText(SavePath);

                // 역직렬화 실패 대비 null 방어
                Data = JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
            }
            catch
            {
                Data = new PlayerData();
            }
        }

        // 이벤트 해제 (메모리 누수 방지)
        private void OnDisable()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.OnRequestAudioSave -= UpdateAudioSettings;

            StageData.OnStagePlayStatusChanged -= HandleStageStatusChanged;
        }
    }
}