using UnityEngine;

public class WaypointData {

	public const string TABLE_NAME = "waypoint_data";

	public int id;
	public int npc;
	public float x;
	public float y;
	public float z;

	// db does not accept Vector3 var type
	public Vector3 position {
		get {
			return new Vector3(x, y, z);
		}
		set {
			x = value.x;
			y = value.y;
			z = value.z;
		}
	}

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		id = Service.db.Id (1);
		return Service.db.Insert (TABLE_NAME, this);
	}

	public void delete () {
		Service.db.Delete (TABLE_NAME, this.id);
	}
}
