using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCData {

	protected string TABLE_NAME = "npc";

	public int id;
	public int template;
	public string name;
	public int level = 1;
	public int health = 100;
	public int _race;
	public int _subRace;
	public creatureRace race {
		get {
			return (creatureRace)_race;
		}
		set {
			_race = (int)value;
		}
	}
	public creatureSubRace subRace {
		get {
			return (creatureSubRace)_subRace;
		}
		set {
			_subRace = (int)value;
		}
	}
	public int damage = 50;
	public int expValue = 1;
	public float attackRange = 2;
	public bool isAggresive = false;
	public float distanceToLoseAggro = 30;
	public float runSpeed = 5;
	public float attacksPerSecond = 1;
	private List<WaypointData> _waypoints = new List<WaypointData> ();
	private bool firstSearch = true;
	public List<WaypointData> waypoints {
		get {
			
			if (firstSearch) {
				foreach (WaypointData data in Service.db.Select<WaypointData>("FROM " + WaypointData.TABLE_NAME + " WHERE npc ==? ORDER BY id asc ", id)) {
					_waypoints.Add(data);
				}
				firstSearch = false;
			}
			
			return _waypoints;
		}
	}

	public enum creatureTemplate
	{
		CastleSpider = 1,
		Human = 2
	}

	public enum creatureRace
	{
		Monster = 1,
		Human = 2
	}

	public enum creatureSubRace {
		Normal = 1,
		Seller = 2,
        Quest = 3,
        Talker = 4
    }

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
