using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * ¿Why is LanguageManager placed in /Plugins/ folder?
 * https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html
 * To give him compilation priority over any other script (so translation is loaded before anything else)
 * LanguageManager.get("phrase here");
**/

public class LanguageManager : MonoBehaviour
{

    public static LanguageManager Instance;
    public static Dictionary<string, string> translation = new Dictionary<string, string>();
    public static Dictionary<string, string> fallbackTranslation = new Dictionary<string, string>();

    private static SystemLanguage fallbackLanguage = SystemLanguage.English;
    public static SystemLanguage[] availableLanguages = {
        SystemLanguage.Spanish,
        SystemLanguage.English,
        SystemLanguage.German,
        SystemLanguage.French,
    };

    public static SystemLanguage? _playerLanguage = null;
    public static SystemLanguage playerLanguage {
        get {
            if (_playerLanguage == null) {
                if (!PlayerPrefs.HasKey("Language")) {
                    PlayerPrefs.SetString("Language", Application.systemLanguage.ToString());
                }

                _playerLanguage = stringToSystemLanguage(PlayerPrefs.GetString("Language"));
            }
            return (SystemLanguage)_playerLanguage;
        }
        set {
            if (System.Array.IndexOf(availableLanguages, value) == -1) {
                Debug.LogError("Attempt to set invalid unavailable language: " + value.ToString());
                return;
            }

            PlayerPrefs.SetString("Language", value.ToString());
            _playerLanguage = value;

            reloadTranslations(true);
        }
    }

    private static bool recordScreenshots = false; // to easy the translation process (have context), take a screenshot everytime that a phrase is shown
    private static TranslationsFile translationFile;
    private static string translationPath = "translation/"; // always end with /
    private static bool gotMissingTranslations = false;
    public static bool isClosing = false;
    public static bool isLoadingTranslations = false;
    public static bool reloadRequested = false;

    void Awake() {
        Instance = this;
        if (translation.Count == 0) {
            reloadTranslations();
        }
    }

    void OnApplicationQuit() {
        isClosing = true;
    }

    public static void reloadTranslations(bool reloadUI = false) {
        isLoadingTranslations = true;
        translation = new Dictionary<string, string>();
        fallbackTranslation = new Dictionary<string, string>();

        if (System.Array.IndexOf(availableLanguages, playerLanguage) != -1) {
            getTranslations(playerLanguage);
        }

        if (Application.systemLanguage != fallbackLanguage) {
            getTranslations(fallbackLanguage, true);
        }
        isLoadingTranslations = false;

        if (reloadUI || reloadRequested) {
            LocalizedText[] translatableTexts = GameObject.FindObjectsOfType<LocalizedText>();

            foreach (LocalizedText text in translatableTexts) {
                text.reload();
            }
        }
    }

    public static SystemLanguage stringToSystemLanguage(string language) {
        return (SystemLanguage)Enum.Parse(typeof(SystemLanguage), language, true);
    }

    public static string get(string key) {
#if UNITY_EDITOR
        if (translation.Count == 0) {
           reloadTranslations();
        }
#else
        if (translation.Count == 0) {
            if (!isLoadingTranslations) {
                reloadTranslations();
            } else {
                reloadRequested = true;
            }
        }
#endif
        if (translation.ContainsKey(key)) {
            return translation[key];
        }

        if (fallbackTranslation.ContainsKey(key)) {
            return fallbackTranslation[key];
        }

#if UNITY_EDITOR
        gotMissingTranslations = true;
        PhraseTranslation phrase = new PhraseTranslation(key, key);
        translation.Add(phrase.key, phrase.translation);
        Debug.Log("NOT TRANSLATED:\n" + JsonUtility.ToJson(phrase, true) + ",\n");

        if (recordScreenshots) {
            if (hasScreenshot(key)) {
                Debug.Log("Screenshot already taken");
            } else {
                Instance.StartCoroutine(takeScreenshot(key));
            }
        }
#endif
        return key;
    }

    /**
    * Save the missing+current translations in a json file
    **/
    void OnDestroy() {
        if (!gotMissingTranslations) {
            return;
        }

        PhraseTranslation[] phrasesList = new PhraseTranslation[translation.Count];
        int i = 0;

        foreach (KeyValuePair<string, string> phrase in translation) {
            phrasesList[i] = new PhraseTranslation(phrase.Key, phrase.Value);
            i++;
        }

        translationFile.translations = phrasesList;
        string json = JsonUtility.ToJson(translationFile, true);

        // remove Translations attr from the json
        string newLine = "\n"; // to not set to Environment.NewLine as JsonUtility.ToJson always sets \n
        json = json.Remove(json.LastIndexOf(newLine));
        json = "[\n" + deleteLines(json, 2);

        setupFolders();
        System.IO.File.WriteAllText(translationPath + playerLanguage.ToString() + ".json", json);
    }

    private static IEnumerator takeScreenshot(string key) {
        yield return new WaitForSeconds(1f);
        setupFolders();
        ScreenCapture.CaptureScreenshot(translationPath + "images/" + MD5(key) + ".png");
        Debug.Log("Screenshot taken");
    }

    private static bool hasScreenshot(string key) {
        return System.IO.File.Exists(translationPath + "images/" + MD5(key) + ".png");
    }

    private static void setupFolders() {
        if (!System.IO.Directory.Exists(translationPath)) {
            System.IO.Directory.CreateDirectory(translationPath);
            System.IO.Directory.CreateDirectory(translationPath + "images/");
        }
    }

    private static string MD5(string strToEncrypt) {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++) {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    private static string deleteLines(string s, int linesToRemove) {
        return s.Split(Environment.NewLine.ToCharArray(),
                       linesToRemove + 1
            ).Skip(linesToRemove)
            .FirstOrDefault();
    }

    private static void getTranslations(SystemLanguage language, bool fallback = false) {
        TranslationsFile file = JsonUtility.FromJson<TranslationsFile>("{\"translations\":" + ((TextAsset)Resources.Load("i18n/" + language.ToString())).text.TrimEnd('\r', '\n') + "}");

        if (file.translations == null) {
            Debug.LogError("Translation file for " + language.ToString() + " is invalid");
            return;
        }

        if (!fallback) {
            translationFile = file;
        }

        foreach (PhraseTranslation phrase in file.translations) {
            try {
                if (fallback) {
                    fallbackTranslation.Add(phrase.key, phrase.translation);
                } else {
                    translation.Add(phrase.key, phrase.translation);
                }
            } catch (Exception) {
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

    public PhraseTranslation(string k, string trans) {
        key = k;
        translation = trans;
    }
}
