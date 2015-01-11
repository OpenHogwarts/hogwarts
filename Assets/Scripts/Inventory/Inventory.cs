using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour, IDragHandler {

	public GameObject itemSlotPrefab;
	public GameObject slotPrefab;
	public GameObject toolTip;
	int slotWidth;
	int slotHeight;
	int margin = 2;

	int x = -110; 
	int y = 110;

	List<Item> items = new List<Item>();

	public static Inventory _instance;
	
	public static Inventory Instance {
		get
		{
			return _instance;
		}
	}

	// Use this for initialization
	void Start () {
		_instance = this;

		int slotNum = 1; // new items have pos 0

		for(int i = 1; i < 6; i++) {
			for(int k = 1; k < 6; k++) {
				GameObject slot = (GameObject)Instantiate(slotPrefab);
				slot.transform.SetParent(this.gameObject.transform, false);
				slot.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
				slot.GetComponent<Slot>().num = slotNum;

				// check if we can fill this slot
				fillSlot(slot.GetComponent<Slot>());

				if (slotWidth == 0) {
					RectTransform rect = slot.GetComponent<RectTransform>();
					slotWidth = (int)rect.rect.width + margin;
					slotHeight = (int)rect.rect.height + margin;
				}

				x = x + slotWidth; 
				if(k == 5)  {
					x = -110;
					y = y - slotHeight;
				}
				slotNum++;
			}
		}

		updateMoney ();

	}

	/**
		Tries to fill the given slot
		@param Slot slot slot to fill
		
		@return void
	 */
	void fillSlot (Slot slot) {

		bool isAssigned = false;
		Item itm = new Item ();

		foreach(CharacterItem characterItem in Menu.db.Select<CharacterItem>("FROM inventory WHERE character == ? & slot == ?", PhotonNetwork.player.customProperties["characterId"], slot.num)) {
			isAssigned = true;
			GameObject itemSlot = (GameObject)Instantiate(itemSlotPrefab);
			itemSlot.GetComponent<ItemSlot>().item = itm.get(characterItem);
			
			itemSlot.transform.SetParent(this.gameObject.transform, false);
			itemSlot.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
			
			break;
		}
		if (!isAssigned) {
			foreach(CharacterItem characterItem in Menu.db.Select<CharacterItem>("FROM inventory WHERE character = ? & slot = ?", PhotonNetwork.player.customProperties["characterId"], 0)) {
				
				// assign this slot to the item
				characterItem.slot = slot.num;
				characterItem.save();
				
				GameObject itemSlot = (GameObject)Instantiate(itemSlotPrefab);
				itemSlot.GetComponent<ItemSlot>().item = itm.get(characterItem);
				
				itemSlot.transform.SetParent(this.gameObject.transform, false);
				itemSlot.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
				break;
			}
			
			if (!isAssigned) {
				slot.available = true;
			}
		}
	}

	public void updateMoney () {
		transform.FindChild ("GalleonLabel").GetComponent<Text> ().text = Player.Instance.galleon.ToString();
		transform.FindChild ("SickleLabel").GetComponent<Text> ().text = Player.Instance.sickle.ToString();
		transform.FindChild ("KnutLabel").GetComponent<Text> ().text = Player.Instance.knut.ToString();
	}

	/**
		Shows a tooltip near to item slot
		@param Vector3 pos Position to show the tooltip
		@param Item item Item which we want to show its information

		@return void
	 */
	public void showTooltip (Vector3 pos, Item item) {
		toolTip.SetActive (true);
		toolTip.GetComponent<RectTransform> ().SetAsLastSibling ();
		toolTip.transform.position = pos;

		toolTip.transform.FindChild("TitleLabel").GetComponent<Text>().text = item.name;
		toolTip.transform.FindChild("TextLabel").GetComponent<Text>().text = item.description;
	}

	/**
		hides the tooltip

		@return void
	*/
	public void hideTooltip () {
		toolTip.SetActive (false);
	}


	/**
		Allows dragging the bag through game screen
		@return void
	*/
	public void OnDrag (PointerEventData eventData) {
		transform.position = Input.mousePosition;
	}
}
