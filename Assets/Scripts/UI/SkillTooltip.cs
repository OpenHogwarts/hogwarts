using UnityEngine;
using System.Collections;

public class SkillTooltip : MonoBehaviour {

	public string cooldown;
	public string name;
	public string description;

	public void Show(){
		Menu.Instance.showSkillTooltip("<size=20>"+name+"</size>\n\n<size=14>"+description+"</size>", cooldown);
	}

	public void Hide(){
		Menu.Instance.SkillTooltip.SetActive (false);
	}
}
