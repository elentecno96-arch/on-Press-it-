using Cysharp.Threading.Tasks;
using Project.Core.Systems.SaveLoad;
using Project.Core.Systems.SaveLoad.Data;
using Project.Core.Utilities;
using Project.Rhythm.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Managers
{
    public class PlayerManager : BaseSingleton<PlayerManager>
    {
        // 책임 분리된 전문가 객체
        private readonly SyncServer _syncServer = new ();

        // 설정값
        public const float CLEAR_SCORE_THRESHOLD = 85000f;

        // UI 이벤트
        public event Action<string> OnNameChanged;
        public event Action<string> OnAccountTagCreated;
        public event Action<float> OnTotalScoreUpdated;
        public event Action<int> OnRankUpdated;

        private bool _isQuittingAndSaved = false;

        // 프로필 정보
        public PlayerData Data { get; private set; }
        public string UserAccountTag => string.IsNullOrEmpty(Data?.userId)
                      ? "#00000"
                      : $"#{Data.userId[..Math.Min(Data.userId.Length, 5)].ToUpper()}";

        protected override void Awake()
        {
            base.Awake();
            Application.wantsToQuit += OnWantsToQuit;
        }

        public override async UniTask Initialize()
        {
            if (IsInitialized) return;

            // 로컬 로드
            Data = SaveSystem.Load();

            // 이벤트 구독
            if (AudioManager.Instance != null)
                AudioManager.Instance.OnRequestAudioSave += UpdateAudioSettings;

            StageData.OnStagePlayStatusChanged += HandleStageStatusChanged;

            await UniTask.Yield();
            IsInitialized = true;

            OnAccountTagCreated?.Invoke(UserAccountTag);
        }

        #region 기록 및 데이터 처리 (DataRecord 활용)

        private void HandleStageStatusChanged(int index, bool isPlayed)
        {
            if (!isPlayed || index <= 0) return;

            // DataRecord를 통해 기록 생성 여부 확인 및 처리
            bool isNew = DataRecord.UpdateStageResult(Data, index, 0);
            if (isNew)
            {
                Save();
                Debug.Log($"[PlayerManager] {index}번 스테이지 기록 생성");
            }
        }

        public void SaveStageResult(int index, float score)
        {
            // 실제 기록 갱신 로직은 DataRecord가 담당
            bool isBestUpdated = DataRecord.UpdateStageResult(Data, index, score);

            if (isBestUpdated)
            {
                float total = GetTotalSumScore();
                OnTotalScoreUpdated?.Invoke(total);

                if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
                    FirebaseManager.Instance.UpdateLeaderboardData(Data.userName, total);
            }

            Save();
        }

        public bool IsStageCleared(int index)
            => DataRecord.IsCleared(Data, index, CLEAR_SCORE_THRESHOLD);

        public List<ScoreRecord> GetTopThreeRecords(int index)
        {
            var detailed = Data.detailedRecords.Find(d => d.stageIndex == index);
            return detailed?.records ?? new List<ScoreRecord>();
        }

        public int GetBestScore(int index)
        {
            return (int)DataRecord.GetBestScore(Data, index);
        }

        #endregion

        #region 저장 및 동기화 (SyncServer / SaveSystem 활용)

        public void Save()
        {
            // 로컬 저장
            SaveSystem.Save(Data);
            // 서버 저장은 SyncServer에게 위임 (쿨타임 포함)
            _syncServer.RequestCloudSave(Data, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public async UniTask SyncWithServer()
        {
            await _syncServer.SyncFromCloud(Data);

            if (FirebaseManager.Instance != null)
            {
                await FirebaseManager.Instance.RefreshMyRankData(GetTotalSumScore());
            }

            OnTotalScoreUpdated?.Invoke(GetTotalSumScore());
            OnNameChanged?.Invoke(Data.userName);

            Save();
        }

        /// <summary>
        /// 앱 종료 요청이 들어올 때 호출됨
        /// </summary>
        private bool OnWantsToQuit()
        {
            // 저장이 이미 완료되었다면 종료 허용
            if (_isQuittingAndSaved) return true;

            // 저장 프로세스 시작 (비동기)
            HandleQuitSave().Forget();
            return false;
        }

        private async UniTaskVoid HandleQuitSave()
        {
            Debug.Log("<color=yellow>[PlayerManager] 앱 종료 전 최종 클라우드 저장 시퀀스 시작...</color>");

            try
            {
                SaveSystem.Save(Data);
                await _syncServer.PerformCloudSaveAsync(Data);

                Debug.Log("<color=green>[PlayerManager] 최종 클라우드 저장 완료. 앱을 안전하게 종료합니다.</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerManager] 종료 저장 중 에러 발생: {e.Message}");
            }
            finally
            {
                _isQuittingAndSaved = true;
                Application.Quit(); // 다시 종료 시도 (이번엔 wantsToQuit에서 true가 반환됨)
            }
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            SaveSystem.Save(Data);
        }

        #endregion

        #region 기타 매니징 로직

        public void UpdateUserName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;
            Data.userName = newName;
            Save();
            OnNameChanged?.Invoke(newName);

            if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
                FirebaseManager.Instance.UpdateLeaderboardData(newName, GetTotalSumScore());
        }

        public void SetUserIdentity(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return;
            Data.userId = uid;
            OnAccountTagCreated?.Invoke(UserAccountTag);
            SyncWithServer().Forget();
        }

        public float GetTotalSumScore()
        {
            float total = 0;
            if (Data?.stageRecords == null) return 0;
            foreach (var s in Data.stageRecords) total += s.bestScore;
            return total;
        }

        private void UpdateAudioSettings(float bgm, float sfx)
        {
            Data.bgmVolume = bgm;
            Data.sfxVolume = sfx;
            Save();
        }

        /// <summary>
        /// 외부(Firebase 등)에서 내 랭킹 정보를 갱신할 때 호출합니다.
        /// </summary>
        /// <param name="newRank">새로운 순위</param>
        public void UpdateRank(int newRank, float newPercent)
        {
            if (Data == null) return;

            Data.currentRank = newRank;
            Data.currentPercent = newPercent;

            OnRankUpdated?.Invoke(newRank);

            Save();
        }

        private void OnDisable()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.OnRequestAudioSave -= UpdateAudioSettings;
            StageData.OnStagePlayStatusChanged -= HandleStageStatusChanged;

            Application.wantsToQuit -= OnWantsToQuit;
        }

        #endregion
    }
}