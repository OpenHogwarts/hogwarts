using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : MonoBehaviour {

    public Text text;

    public void setText (string message) {
        text.text = message;
    }

    public void showNPCText (int npcId) {
        text.text = DBSetup.getTalkerPhrase(npcId);
    }
}
