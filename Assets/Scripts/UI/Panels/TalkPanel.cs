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
            case 6: // regular student
                text.text = "Estoy deseando retomar las clases";
                break;
            case 7: // regular student
                text.text = "Cuando vayas a usar los baños, recuerda que los cuadros te observan";
                break;
        }
    }
}
