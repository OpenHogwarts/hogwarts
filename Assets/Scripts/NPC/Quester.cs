using UnityEngine;
using System.Collections.Generic;

public class Quester : NPC
{
    public override void OnClick()
    {
        List<int> quests = QuestManager.Instance.getByNPC(Id);

        GameObject panel = Menu.Instance.showPanel("QuestPanel", false);
        panel.GetComponent<QuestPanel>().setQuest(QuestManager.Instance.allQuests[quests[0]]);
    }
}