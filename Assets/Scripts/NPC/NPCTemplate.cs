using UnityEngine;
using System.Collections;

public class NPCTemplate {

	protected string TABLE_NAME = "npc_template";

	public int id;
	public string name;
	public int healthBase = 100; // health at level 1
	public bool isAgressive = false;

	public int _creatureRace;
	public int _creatureSubRace;

	public NPCData.creatureRace creatureRace {
		get {
			return (NPCData.creatureRace)_creatureRace;
		}
		set {
			_creatureRace = (int)value;
		}
	}
	public NPCData.creatureSubRace creatureSubRace {
		get {
			return (NPCData.creatureSubRace)_creatureSubRace;
		}
		set {
			_creatureSubRace = (int)value;
		}
	}

	public static NPCTemplate get (int id) {
		return Service.db.SelectKey<NPCTemplate> ("npc_template", id);
	}

	public static NPCData fillById (NPCData.creatureTemplate id, int level)
	{
		int _id = (int)id;
		NPCData npc = new NPCData();
		NPCTemplate template = NPCTemplate.get(_id);

		npc.name = template.name;
		npc.template = _id;
		npc.race = template.creatureRace;
		npc.subRace = template.creatureSubRace;
		npc.level = level;
		npc.health = template.healthBase * level;
		npc.isAggresive = template.isAgressive;

		return npc;
	}

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
