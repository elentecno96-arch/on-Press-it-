using Project.Core.Systems.SaveLoad.Data;
using System;
using System.IO;
using UnityEngine;

namespace Project.Core.Systems.SaveLoad
{
    /// <summary>
    /// 저장/불러오기 전담 시스템
    /// C# 객체(데이터)를 로컬 하드디스크의 JSON 파일로 바꾸고, 다시 읽어오는 물리적인 입출력
    /// </summary>
    public static class SaveSystem
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "PlayerSave.json");

        /// <summary>
        /// 데이터를 로컬 JSON 파일로 저장
        /// </summary>
        public static void Save(PlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] 로컬 저장 성공: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 저장 중 오류 발생: {e.Message}");
            }
        }

        /// <summary>
        /// 로컬 JSON 파일을 읽어 데이터로 변환, 파일이 없으면 새 데이터를 반환
        /// </summary>
        public static PlayerData Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[SaveSystem] 기존 저장 파일이 없습니다. 새로 생성합니다.");
                return new PlayerData();
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 로드 중 오류 발생: {e.Message}");
                return new PlayerData();
            }
        }

        /// <summary>
        /// 세이브 데이터를 초기화(삭제)합니다.
        /// </summary>
        public static void ClearSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        /// <summary>
        /// //데이터를 직렬화
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize(PlayerData data)
        {
            return JsonUtility.ToJson(data);
        }
    }
}
