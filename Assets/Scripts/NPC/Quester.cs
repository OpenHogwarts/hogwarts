using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Quester : NPC
{

    new public void OnMouseDown()
    {
        List<int> quests = QuestManager.Instance.getByNPC(Id);

        GameObject panel = Menu.Instance.showPanel("QuestPanel", false);
        panel.GetComponent<QuestPanel>().setQuest(QuestManager.Instance.allQuests[quests[0]]);

        base.OnMouseDown();
    }
}