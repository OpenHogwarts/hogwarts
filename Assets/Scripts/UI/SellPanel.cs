using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SellPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ScrollableList scroll = gameObject.transform.FindChild("ContainerPanel/Panel").GetComponent<ScrollableList>();
		scroll.load(delegate (GameObject newItem, int num) {
			
			newItem.gameObject.transform.FindChild("NameLabel").GetComponent<Text>().text = "df";
			
			return newItem;
		});
	}
}
