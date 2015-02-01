using UnityEngine;
using System.Collections;

public class NPCData {

	protected string TABLE_NAME = "npc";

	public int id;
	public string name;
	public int level = 1;
	public int health = 100;
	public int _type;
	public int _subType;
	public creatureType type {
		get {
			return (creatureType)_type;
		}
		set {
			_type = (int)value;
		}
	}
	public creatureSubType subType {
		get {
			return (creatureSubType)_subType;
		}
		set {
			_subType = (int)value;
		}
	}
	public int damage = 50;
	public int expValue = 1;
	public float attackRange = 1;
	public bool isAggresive = false;
	public float distanceToLoseAggro = 30;
	public float runSpeed = 8;
	public float attacksPerSecond = 1;

	public enum creatureType
	{
		Monster = 1,
		Human = 2
	}

	public enum creatureSubType {
		Normal = 1,
		Seller = 2,
	}

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
