using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	private List<WaypointData> _waypoints = new List<WaypointData> ();
	public List<WaypointData> waypoints {
		get {
			
			if (_waypoints.Count == 0) {
				foreach (WaypointData data in Service.db.Select<WaypointData>("FROM "+WaypointData.TABLE_NAME + " WHERE npc ==? ORDER BY id asc ", id)) {
					_waypoints.Add(data);
				}
			}
			
			return _waypoints;
		}
	}

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
