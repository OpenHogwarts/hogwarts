using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Menu : MonoBehaviour {

	public static string defaultLevel = "Hogwarts";
	public static string debugLevel = "Test";

	public List<GameObject> Menus;
	
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

	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
		DontDestroyOnLoad(GameObject.Find("EventSystem"));
	}

	public void OnLevelWasLoaded(int level) {
		switch (level) {
		case 0: // Main
			showPanel("MainPanel");
			break;
		default:
			showPanel("PlayerPanel");
			showPanel("ChatPanel", false);
			showPanel("TopMenu", false);
			showPanel("MiniMap", false);
			break;
		}
	}
	
	public GameObject showPanel (string name, bool hidePanels = true) {
		if (hidePanels) {
			hideAllPanels ();
		}
		
		GameObject panel = this.getPanel (name);
		panel.SetActive (true);
		
		return panel;
	}

	public GameObject getPanel (string name) {
		foreach (GameObject panel in Menus) {
			if (panel.name == name) {
				return panel;
			}
		}
		throw new UnityException ("UI Panel "+ name +" not found");
	}
	
	public void togglePanel (string name) {
		GameObject panel = getPanel (name);
		
		panel.SetActive (!panel.GetActive());
	}
	
	public void hideAllPanels() {
		foreach (GameObject panel in Menus) {
			panel.SetActive(false);
		}
	}
}