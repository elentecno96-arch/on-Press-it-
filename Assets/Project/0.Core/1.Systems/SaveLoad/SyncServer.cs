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

            // 서버에서 데이터를 가져옴
            string json = await FirebaseManager.Instance.LoadPlayerData();

            // 데이터가 없으면 에러 없이 종료
            if (string.IsNullOrEmpty(json) || json == "null")
            {
                Debug.Log("[SyncServer] 서버 데이터가 비어있습니다. 신규 유저로 간주합니다.");
                return;
            }

            // 데이터가 존재하지만 JSON 형식이 아닐 경우 방어
            if (!json.StartsWith("{"))
            {
                Debug.LogWarning($"[SyncServer] 서버 데이터가 올바른 객체 형식이 아닙니다: {json}");
                return;
            }

            try
            {
                PlayerData serverData = JsonUtility.FromJson<PlayerData>(json);
                if (serverData != null)
                {
                    localData.Merge(serverData);
                    Debug.Log("[SyncServer] 서버 데이터 병합 완료!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SyncServer] JSON 파싱 중 오류 발생: {e.Message}");
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

            PerformCloudSaveAsync(data).Forget();
        }

        // 실제 서버 전송 실행
        public async UniTask PerformCloudSaveAsync(PlayerData data)
        {
            if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized) return;

            _lastSaveTime = Time.time;
            string json = SaveSystem.Serialize(data);
            await FirebaseManager.Instance.SavePlayerData(json);
            
            Debug.Log("[SyncServer] 서버 저장 완료");
        }
    }
}
