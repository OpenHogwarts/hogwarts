using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using iBoxDB.LocalServer;

public class Service {

	public static DB server = null;
	public static DB.AutoBox _db;
	public static DB.AutoBox db {
		get { 
			if (_db == null) {

                #if UNITY_EDITOR
                //Debug.Log(Application.persistentDataPath);
                DB.Root (Application.persistentDataPath);
				#endif

				#if (UNITY_METRO || NETFX_CORE) && (!UNITY_EDITOR)
				// from WSDatabaseConfig.cs
				iBoxDB.WSDatabaseConfig.ResetStorage();
				#endif
				server = new DB (3);

				bool isUnique = true;
				bool isNotUnique = false;
				
				server.GetConfig ().EnsureTable<CharacterData> ("characters", "id");
				server.GetConfig ().EnsureIndex<CharacterData> ("characters", isUnique, "name");
				
				server.GetConfig ().EnsureTable<CharacterItem> ("inventory", "item");

				//server.GetConfig ().EnsureIndex<CharacterItem> ("inventory", isUnique, "_position");
				server.GetConfig ().EnsureTable<Item> ("item", "id");
				server.GetConfig ().EnsureTable<NPCData> ("npc", "id");
				server.GetConfig ().EnsureTable<NPCTemplate> ("npc_template", "id");
				server.GetConfig ().EnsureTable<WaypointData> ("waypoint_data", "id");
				server.GetConfig ().EnsureIndex<WaypointData> ("waypoint_data", isNotUnique, "npc");

                server.GetConfig().EnsureTable<Task>("tasks", "taskId");
                server.GetConfig().EnsureIndex<Task>("tasks", isNotUnique, "quest");

                _db = server.Open ();
			}
			return _db;
		}
		set {}
	}

	/**
	 * Gets one result from db query
	 * 
	 */
	public static T getOne<T> (string query, params object[] args) where T:class,new() {
		
		IEnumerator<T> enumerator = Service.db.Select<T> (query+" limit 0,1", args).GetEnumerator ();
		enumerator.MoveNext ();
		return enumerator.Current;
	}
}
