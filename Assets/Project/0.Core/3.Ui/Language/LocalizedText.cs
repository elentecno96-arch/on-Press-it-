using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    public string key;

    TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        var manager = LocalizationManager.Instance;

        text.text = manager.GetText(key);
        text.font = manager.GetFont();
    }
}
