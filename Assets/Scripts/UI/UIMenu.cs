using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
	Basic menu to toggle ingame UI panels
 */

public class UIMenu : MonoBehaviour {

	public GameObject BagPanel;
	public GameObject SellerPanel;
	public GameObject CharacterPanel;
	public GameObject ItemTooltipPanel;

	public static UIMenu _instance;
	
	public static UIMenu Instance {
		get
		{
			return _instance;
		}
	}
	
	public void Start () {
		_instance = this;
	}

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
		SellerPanel.SetActive (false);
		CharacterPanel.SetActive (false);
	}

	/**
		Shows a tooltip near to item slot
		@param Vector3 pos Position to show the tooltip
		@param Item item Item which we want to show its information

		@return void
	 */
	public void showTooltip (Vector3 pos, Item item) {
		ItemTooltipPanel.SetActive (true);
		ItemTooltipPanel.GetComponent<RectTransform> ().SetAsLastSibling ();
		ItemTooltipPanel.transform.position = pos;
		
		ItemTooltipPanel.transform.FindChild("TitleLabel").GetComponent<Text>().text = item.name;
		ItemTooltipPanel.transform.FindChild("TextLabel").GetComponent<Text>().text = item.description;
	}
	
	/**
		hides the tooltip

		@return void
	*/
	public void hideTooltip () {
		ItemTooltipPanel.SetActive (false);
	}
}
