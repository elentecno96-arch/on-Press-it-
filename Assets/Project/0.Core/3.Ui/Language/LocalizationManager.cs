using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Language
{
    Korean,
    English,
    Japanese
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public Language currentLanguage = Language.Korean;

    private Dictionary<string, string[]> data = new Dictionary<string, string[]>();

    public TMP_FontAsset koreanFont;
    public TMP_FontAsset englishFont;
    public TMP_FontAsset japaneseFont;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        LoadCSV();
    }

    void LoadCSV()
    {
        TextAsset csv = Resources.Load<TextAsset>("Font/Localization");

        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            string key = values[0];

            string[] langs = new string[3];
            langs[0] = values[1]; // Korean
            langs[1] = values[2]; // English
            langs[2] = values[3]; // Japanese

            data[key] = langs;
        }
    }

    public string GetText(string key)
    {
        if (!data.ContainsKey(key))
            return key;

        return data[key][(int)currentLanguage];
    }

    public void ChangeLanguage(Language lang)
    {
        currentLanguage = lang;

        Debug.Log("언어 변경: " + lang);

        UpdateAllTexts();
    }

    void UpdateAllTexts()
    {
        LocalizedText[] texts = FindObjectsOfType<LocalizedText>();

        foreach (var t in texts)
        {
            t.UpdateText();
        }
    }

    public TMP_FontAsset GetFont()
    {
        switch (currentLanguage)
        {
            case Language.Korean: return koreanFont;
            case Language.English: return englishFont;
            case Language.Japanese: return japaneseFont;
        }

        return koreanFont;
    }
}
