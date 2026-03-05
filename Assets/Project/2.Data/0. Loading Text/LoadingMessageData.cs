using UnityEngine;

namespace Project.Data.LoadingText
{
    [CreateAssetMenu(fileName = "LoadingMessageData", menuName = "Project/Data/LoadingMessageData")]
    public class LoadingMessageData : ScriptableObject
    {
        //로딩 화면에 배치할 텍스트들
        [SerializeField] private string[] messages;

        public string GetRandomMessage()
        {
            if (messages == null || messages.Length == 0) return string.Empty;
            return messages[Random.Range(0, messages.Length)];
        }
    }
}
