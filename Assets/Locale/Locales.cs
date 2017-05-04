using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Locales : MonoBehaviour {
    /*=======================[ USAGE ]==================
     * To get the Name, Pre, After for each quest in a particular Locale
     * 
     * GetQuestData(int _questID, Locale _language)
     * 
     *  @Returns string[3]
     *      string[0] = Name
     *      string[1] = Pre
     *      string[2] = After
     * 
     * 
     * 
     */


    public static Dictionary<int, int> QuestDB; // QuestID, ReferenceID
    public static Dictionary<int, string[]> QuestData; // ReferenceID, 6 Length Array.
    
    /*
        
    */
    
    public enum Locale {
        Spanish,
        English
    }
    public static Locale GetLocaleByString(string _language) {
        if (_language == "es") return Locale.Spanish;
        if (_language == "es_en") return Locale.English;
        return Locale.Spanish; // Spanish as Default or invalid language
    }

    public static Locale ConvertPlayerLocale() {
        return GetLocaleByString(PlayerPrefs.GetString("Locale"));
    }
    public static void InitializeDatabase() {
        

    }

    public static string[] GetQuestData(int _questID, Locale _language) {
        int ReferenceID = 0;
        if (!QuestDB.TryGetValue(_questID, out ReferenceID)) {
            return new string[] { "Invalid Quest", "Quest with that ID cannot be found.", "And His Name is John Cena!" };
        }
        // Locale, Locale+1, Locale+2
        return new string[] { QuestData[_questID][(int)_language], QuestData[_questID][(int)_language+1], QuestData[_questID][(int)_language+2] };
    }

    public static int GetHighestReferenceID() {
        var largestNumber = 0;
        foreach (KeyValuePair<int, string[]> reference in QuestData) {
            if (largestNumber < reference.Key)
                largestNumber = reference.Key;
        }
        return largestNumber;
    }

    public static bool AddQuestData(int _questID, Locale _language, string[] _questData) {
        int ReferenceID = 0;
        if (_questData.Length < 3) { Debug.Log("Quest Data < 3 values"); return false; }
        if (!QuestDB.TryGetValue(_questID, out ReferenceID)) {
            // Quest Doesn't Exist

            // How many languages are supported?
            var LocaleMemberCount = Enum.GetNames(typeof(Locale)).Length;
            // (_questData.Length % 3) should be zero, since we need multiples of 3. So if it gets a remainder, them oops.
            if (_questData.Length > 3 && (_questData.Length % 3) != 0) {
                Debug.Log("Quest Data Contains Incomplete Multiples of 3. Incomplete Translations. (ID=" + _questID + ")");
                return false;
            }
            // Get The Highest Number of Quest ID's and 
            ReferenceID = GetHighestReferenceID() + 1;
            string[] tempQuestData = new string[_questData.Length];
            // We have multiples of 3 quest entries, just start start at zero and go TO THE TOP!
            for (int cnt = 0; cnt <= _questData.Length; cnt++) {
                tempQuestData[cnt] = _questData[cnt];
            }
            QuestDB.Add(_questID, ReferenceID);
            QuestData.Add(ReferenceID, tempQuestData);

            // We have created the values.

        } else {
            // Quest Exists -> UPDATE ALL THE THINGS!

            // How many languages are supported?
            var LocaleMemberCount = Enum.GetNames(typeof(Locale)).Length;
            // (_questData.Length % 3) should be zero, since we need multiples of 3. So if it gets a remainder, them oops.
            if (_questData.Length > 3 && (_questData.Length % 3) != 0) {
                Debug.Log("Quest Data Contains Incomplete Multiples of 3. Incomplete Translations. (ID=" + _questID + ")");
                return false;
            }
            // We have multiples of 3 quest entries, just start start at zero and go TO THE TOP!
            for (int cnt = 0; cnt <= _questData.Length; cnt++) {
                QuestData[ReferenceID][cnt] = _questData[cnt];
            }

        }

        return true;
        
    }

   

    

}
