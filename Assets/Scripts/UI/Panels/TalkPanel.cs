using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : MonoBehaviour {

    public Text text;

    public void setText (string message) {
        text.text = message;
    }

    public void showNPCText (int npcId) {

        switch (npcId) {
            case 5: // regular student
                text.text = LanguageManager.get("RANDOM_STUDENT_PHRASE_1");
                break;
            case 6: // regular student
                text.text = LanguageManager.get("RANDOM_STUDENT_PHRASE_2");
                break;
            case 7: // regular student
                text.text = LanguageManager.get("RANDOM_STUDENT_PHRASE_3");
                break;
        }
    }
}
