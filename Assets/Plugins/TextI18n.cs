using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/TextI18n", 10)]
public class TextI18n : Text
{
    void Awake()
    {
        if (color == Color.white)
        {
            color = new Color32(50, 50, 50, 255);
        }

        Refresh();
    }

    public void Refresh()
    {
        // in editor request the phrase without applying to easily see if there is any missing phrase in json files (LanguageManager.cs#117 aprox)
#if UNITY_EDITOR
        LanguageManager.get(text);
#else
        text = LanguageManager.get(text);
#endif
    }
}