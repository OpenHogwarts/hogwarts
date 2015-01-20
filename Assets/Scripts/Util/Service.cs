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
				DB.Root (Application.persistentDataPath);
				#if (UNITY_METRO || NETFX_CORE) && (!UNITY_EDITOR)
				// from WSDatabaseConfig.cs
				iBoxDB.WSDatabaseConfig.ResetStorage();
				#endif
				server = new DB (3);
				
				server.GetConfig ().EnsureTable<CharacterData> ("characters", "id");
				bool isUniqueName = true;
				server.GetConfig ().EnsureIndex<CharacterData> ("characters", isUniqueName, "name");
				
				server.GetConfig ().EnsureTable<CharacterItem> ("inventory", "id");
				server.GetConfig ().EnsureTable<Item> ("item", "id");
				
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
