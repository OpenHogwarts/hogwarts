using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateObjects : Editor {

	[MenuItem("Spell Maker/Create Objects")]

	static void Init()
	{
		GameObject spellManager = new GameObject ("SpellManager");
		spellManager.AddComponent <SpellManager>();

	}
}
