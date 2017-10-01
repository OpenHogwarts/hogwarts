using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * ¿Why is LanguageManager placed in /Plugins/ folder?
 * https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html
 * To give him compilation priority over any other script (so translation is loaded before anything else)
**/

public class LanguageManager : MonoBehaviour
{

    public static Dictionary<string, string> translation = new Dictionary<string, string>();
    public static Dictionary<string, string> fallbackTranslation = new Dictionary<string, string>();

    private static SystemLanguage fallbackLanguage = SystemLanguage.English;
    public static SystemLanguage[] availableLanguages = {
        SystemLanguage.Spanish,
        SystemLanguage.English,
    };

    public static SystemLanguage? _playerLanguage = null;
    public static SystemLanguage playerLanguage
    {
        get
        {
            if (_playerLanguage == null)
            {
                if (!PlayerPrefs.HasKey("Language"))
                {
                    PlayerPrefs.SetString("Language", Application.systemLanguage.ToString());
                }
                _playerLanguage = stringToSystemLanguage(PlayerPrefs.GetString("Language"));
            }
            return (SystemLanguage)_playerLanguage;
        }
        set
        {
            if (System.Array.IndexOf(availableLanguages, value) == -1)
            {
                Debug.LogError("Attempt to set invalid unavailable language: " + value.ToString());
                return;
            }

            PlayerPrefs.SetString("Language", value.ToString());
            _playerLanguage = value;

            reloadTranslations(true);
        }
    }

    void Awake()
    {
        if (translation.Count == 0)
        {
            reloadTranslations();
        }
    }

    public static void reloadTranslations(bool reloadUI = false)
    {
        translation = new Dictionary<string, string>();
        fallbackTranslation = new Dictionary<string, string>();

        if (System.Array.IndexOf(availableLanguages, playerLanguage) != -1)
        {
            getTranslations(playerLanguage);
        }

        if (Application.systemLanguage != fallbackLanguage)
        {
            getTranslations(fallbackLanguage, true);
        }

        if (reloadUI)
        {
            TextI18n[] translatableTexts = GameObject.FindObjectsOfType<TextI18n>();

            foreach (TextI18n text in translatableTexts)
            {
                text.Refresh();
            }
        }
    }

    public static SystemLanguage stringToSystemLanguage(string language)
    {
        return (SystemLanguage)Enum.Parse(typeof(SystemLanguage), language, true);
    }

    public static string get(string key)
    {
        if (key == "") {
            return "";
        }
#if UNITY_EDITOR
        if (translation.Count == 0)
        {
            reloadTranslations();
        }
#endif
        if (translation.ContainsKey(key))
        {
            return translation[key];
        }

        if (translation.ContainsKey(key))
        {
            return fallbackTranslation[key];
        }

#if UNITY_EDITOR
        PhraseTranslation phrase = new PhraseTranslation();
        phrase.key = key;
        phrase.translation = key;
        Debug.Log("NOT TRANSLATED:\n" + JsonUtility.ToJson(phrase, true) + ",\n");
#endif
        return key;
    }

    private static void getTranslations(SystemLanguage language, bool fallback = false)
    {
        TranslationsFile file = JsonUtility.FromJson<TranslationsFile>("{\"translations\":" + ((TextAsset)Resources.Load("i18n/" + language.ToString())).text.TrimEnd('\r', '\n') + "}");

        if (file.translations == null)
        {
            Debug.LogError("Translation file for " + language.ToString() + " is invalid");
            return;
        }

        foreach (PhraseTranslation phrase in file.translations)
        {
            try
            {
                if (fallback)
                {
                    fallbackTranslation.Add(phrase.key, phrase.translation);
                } else
                {
                    translation.Add(phrase.key, phrase.translation);
                }
            } catch (Exception)
            {
                Debug.LogError("Duplicated key " + phrase.key + " for " + language.ToString());
            }
        }
    }
}

[System.Serializable]
public class TranslationsFile
{
    public PhraseTranslation[] translations;
}

[System.Serializable]
public class PhraseTranslation
{
    public string key;
    public string translation;
}