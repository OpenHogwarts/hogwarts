using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TaskUI : MonoBehaviour
{
    public Text taskName;
    public Text taskStatus;

	public void setName (string name) {
        taskName.text = name;
    }
    public void setStatus (bool completed) {
        if (completed) {
            taskStatus.text = "✔";
        } else {
            taskStatus.text = "✘";
        }
    }
}
