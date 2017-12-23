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
                text.text = "¿Sabías que el que no deber ser nombrado estudió en este colegio?";
                break;
        }
    }
}
