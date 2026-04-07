using System.Collections.Generic;
using UnityEngine;

namespace Project.UI.Profile.Data
{
    /// <summary>
    /// 프로필 도움말 SO
    /// </summary>
    [CreateAssetMenu(fileName = "HelpData", menuName = "Project/UI/HelpData")]
    public class HelpDataSO : ScriptableObject
    {
        [TextArea(5, 10)] public string description;
    }
}