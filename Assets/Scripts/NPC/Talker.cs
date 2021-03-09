using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talker : NPC {

    public override void OnClick() {
        Menu.Instance.showPanel("TalkPanel", false).GetComponent<TalkPanel>().showNPCText(Id);
    }
}
