using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;


public class Menu : MonoBehaviour {



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