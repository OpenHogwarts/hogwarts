using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using iBoxDB.LocalServer;

public class Menu : MonoBehaviour {

	public DB server = null;
	public static DB.AutoBox db = null;

	public GameObject OptionsPanel;
	public GameObject MainPanel;
	public GameObject CreatePanel;
	
	public const string GAME_VERSION = "0.01";

	public static Menu _instance;
	
	public static Menu Instance {
		get
		{
			return _instance;
		}
	}

	void Start () {
		_instance = this;

		if (db == null) {
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
			
			db = server.Open ();
		}
	}

	public static T getOne<T> (string query, params object[] args) where T:class,new() {

		IEnumerator<T> enumerator = Menu.db.Select<T> (query+" limit 0,1", args).GetEnumerator ();
		enumerator.MoveNext ();
		return enumerator.Current;
	}
	
	public void showPanel (string name) {
		hideAllPanels ();
		
		GameObject panel = (GameObject)this.GetType ().GetField (name).GetValue (this);
		panel.SetActive (true);
	}
	
	public void hideAllPanels() {
		CreatePanel.SetActive (false);
		//OptionsPanel.SetActive(false);
		MainPanel.SetActive(false);
	}
}