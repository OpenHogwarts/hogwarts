using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestPanel : MonoBehaviour
{
    public Text title;
    public Text text;
    public GameObject acceptButton;
    public GameObject completeButton;

    private Quest quest;

    public void setQuest(Quest questData)
    {
        quest = questData;
        title.text = quest.name;

        if (quest.isCompleted) {
            text.text = processText(quest.after);
            acceptButton.SetActive(false);
            completeButton.SetActive(true);
        } else if (QuestManager.Instance.quests.ContainsKey(quest.id)) { // quest accepted, but not finished
            text.text = "Aún... Sigues aquí?... ¿Qué tal si vas por ahí a completarla?";
        } else {
            text.text = processText(quest.pre);
            acceptButton.SetActive(true);
            completeButton.SetActive(false);
        }
    }

    private string processText (string message)
    {
        message = message.Replace("{{username}}", Player.Instance.characterData.name);

        return message;
    }

    public void OnAccept ()
    {
        QuestManager.Instance.addQuest(quest);

        text.text = "¡Perfecto!";
        acceptButton.SetActive(false);
    }

    public void OnAcceptReward()
    {
        QuestManager.Instance.completeQuest(quest);

        text.text = "¡Perfecto!";
        acceptButton.SetActive(false);
    }
}