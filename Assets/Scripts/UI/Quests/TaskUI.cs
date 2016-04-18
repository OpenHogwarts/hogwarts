using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TaskUI : MonoBehaviour
{
    private string text;

	public void setPhrase (string phrase) {
        text = phrase;
    }
    public void setStatus (bool completed) {
        if (completed) {
			GetComponent<Text>().text = text + " ✔";
        } else {
			GetComponent<Text>().text = text + " ✘";
        }
    }
}
