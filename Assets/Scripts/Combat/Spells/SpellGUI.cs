using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellGUI : MonoBehaviour {

	// So basic gui showing spells icons,infos,damages, etc... based on category.
	public List<Spell> spellList = new List<Spell>();
	public bool open = false;
	public bool showFireSpells = false;
	public bool showFrostSpells = false;

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.P))
			open = !open;

	}

	// Update is called once per frame
	void OnGUI () {

		if(open)
		GUI.Window (0, new Rect (200, 200, 400, 400), SpellWindow, "Spells");
	
	}

	public void SpellWindow(int windowID)
	{

		if(GUILayout.Button ("Fire"))
		{
			showFireSpells = true;
			showFrostSpells = false;
		}
		if(GUILayout.Button("Frost"))
		{
			showFireSpells = false;
			showFrostSpells = true;
		}

		if(showFireSpells){
			for(int i = 0; i < spellList.Count; i++)
			{
				if(spellList[i].spellCategory == Spell.SpellCategory.Fire){
					GUILayout.BeginHorizontal();
					GUILayout.Label(spellList[i].spellIcon,GUILayout.MaxWidth(48),GUILayout.MaxHeight(48));
					GUILayout.Label(spellList[i].spellName + " - " + spellList[i].spellInfo);
					GUILayout.Label(spellList[i].spellMinDamage.ToString() + "," + spellList[i].spellMaxDamage.ToString());
					GUILayout.EndHorizontal();
				}
			}
		}

		if(showFrostSpells){
			for(int i = 0; i < spellList.Count; i++)
			{
				if(spellList[i].spellCategory == Spell.SpellCategory.Frost){
					GUILayout.BeginHorizontal();
					GUILayout.Label(spellList[i].spellIcon,GUILayout.MaxWidth(48),GUILayout.MaxHeight(48));
					GUILayout.Label(spellList[i].spellName + " - " + spellList[i].spellInfo);
					GUILayout.Label(spellList[i].spellMinDamage.ToString() + "," + spellList[i].spellMaxDamage.ToString());
					GUILayout.EndHorizontal();
				}
			}
		}


	}

}
