using UnityEngine;
using System.Collections;

/**
	Basic menu to toggle ingame UI panels
 */

public class UIMenu : MonoBehaviour {

	public GameObject BagPanel;

	public void togglePanel (string name) {
		GameObject panel = (GameObject)this.GetType ().GetField (name).GetValue (this);
		panel.SetActive (!panel.GetActive());
	}

	public void showPanel (string name) {
		
		GameObject panel = (GameObject)this.GetType ().GetField (name).GetValue (this);
		panel.SetActive (true);
	}
	
	public void hideAllPanels() {
		BagPanel.SetActive (false);
	}
}
