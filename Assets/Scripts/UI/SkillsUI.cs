using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillsUI : MonoBehaviour {

	public static SkillsUI Instance;

	public List<Button> Skills;

	// Use this for initialization
	void Start () {
		Instance = this;
	}

	public void execSkill (int num) {
		num--;
		PlayerCombat.Instance.spellCast(num);
	}

	public void disableSkill (int num) {
		Skills[num].interactable = false;
	}

	public void enableSkill (int num) {
		Skills[num].interactable = true;
	}
}
