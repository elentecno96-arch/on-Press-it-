using Cysharp.Threading.Tasks;
using Project.Core.Systems.SaveLoad.Data;
using System;
using System.Threading;
using UnityEngine;

namespace Project.Core.Systems.SaveLoad
{
    /// <summary>
    /// 플레이어 데이터 서버 연결 및 동기화 관리
    /// </summary>
    public class SyncServer
    {
        private float _lastSaveTime;
        private const float SAVE_COOLDOWN = 0.5f;
        private bool _isSavePending;

        // 서버 데이터 로드 및 병합
        public async UniTask SyncFromCloud(PlayerData localData)
        {
            if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized) return;

            string json = await FirebaseManager.Instance.LoadPlayerData();
            if (!string.IsNullOrEmpty(json))
            {
                PlayerData serverData = JsonUtility.FromJson<PlayerData>(json);
                localData.Merge(serverData);
            }
        }

        // 서버 저장 요청 (쿨타임 포함)
        public async UniTaskVoid RequestCloudSave(PlayerData data, CancellationToken token)
        {
            if (_isSavePending) return;

            float elapsed = Time.time - _lastSaveTime;
            if (elapsed < SAVE_COOLDOWN)
            {
                _isSavePending = true;
                await UniTask.Delay(TimeSpan.FromSeconds(SAVE_COOLDOWN - elapsed), cancellationToken: token);
                _isSavePending = false;
            }

            PerformCloudSave(data);
        }

        // 실제 서버 전송 실행
        public void PerformCloudSave(PlayerData data)
        {
            if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized) return;

            _lastSaveTime = Time.time;
            string json = SaveSystem.Serialize(data);
            FirebaseManager.Instance.SavePlayerData(json);
        }
    }
}
