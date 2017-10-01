using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPanel : MonoBehaviour {

	public GameObject itemSlotPrefab;

	// Use this for initialization
	void OnEnable () {
		reload ();
	}

	public void reload () {
		GameObject itemSlot;
		Item itm = new Item ();

		destroyOldIcons ();

		foreach (CharacterItem characterItem in Service.db.Select<CharacterItem>("FROM inventory WHERE _position != ? & character == ?", 0, PhotonNetwork.player.customProperties["characterId"])) {

			Slot slot  = this.transform.Find("Slot"+characterItem._position).GetComponent<Slot>();
			slot.available = false;
			itemSlot = (GameObject)Instantiate(itemSlotPrefab);
			itemSlot.tag = "TemporalPanel";
			itemSlot.GetComponent<ItemSlot>().item = itm.get(characterItem);
			itemSlot.GetComponent<ItemSlot>().currentSlot = slot;
			
			itemSlot.transform.SetParent(this.gameObject.transform, false);
			itemSlot.GetComponent<RectTransform>().localPosition = slot.transform.localPosition;
		}
	}


	void destroyOldIcons () {
		var children = new List<GameObject>();
		foreach (Transform child in transform) {
			if (child.tag == "TemporalPanel") {
				children.Add(child.gameObject);
			}
		} 
		children.ForEach(child => Destroy(child));
	}

}
