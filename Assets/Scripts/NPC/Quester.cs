using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Quester : NPC
{

    new public void OnMouseDown()
    {
        var distanceToTarget = Vector3.Distance(transform.position, Player.Instance.transform.position);
        if(distanceToTarget >= 5.0f) {
            // Alert User Somehow?
            setSelected(true);
            Debug.Log("[User Cannot See This] Target Too Far Away To Interact With.");
            return;
        }
        List<int> quests = QuestManager.Instance.getByNPC(Id);

        GameObject panel = Menu.Instance.showPanel("QuestPanel", false);
        panel.GetComponent<QuestPanel>().setQuest(QuestManager.Instance.allQuests[quests[0]]);

        base.OnMouseDown();
    }
}