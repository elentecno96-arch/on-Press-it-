using UnityEngine;

public class LanguageChanger : MonoBehaviour
{
    public void SetKorean()
    {
        LocalizationManager.Instance.ChangeLanguage(Language.Korean);
    }

    public void SetEnglish()
    {
        LocalizationManager.Instance.ChangeLanguage(Language.English);
    }

    public void SetJapanese()
    {
        LocalizationManager.Instance.ChangeLanguage(Language.Japanese);
    }
}
