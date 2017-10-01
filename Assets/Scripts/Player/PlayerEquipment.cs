using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerEquipment : MonoBehaviour {

	public GameObject head;
	public GameObject handLeft;
	public GameObject handRight;

	public Dictionary<int, Item> equipedItems = new Dictionary<int, Item>();

	public static PlayerEquipment _instance;
	
	public static PlayerEquipment Instance {
		get
		{
			return _instance;
		}
	}


	// Use this for initialization
	void Start () {
		_instance = this;

		reload ();
	}

	public void reload () {
		Item item;
		GameObject obj;
		Dictionary<int, bool> loadedPositions = new Dictionary<int, bool>();

		foreach (CharacterItem characterItem in Service.db.Select<CharacterItem>("FROM inventory WHERE _position != ? & character == ?", 0, PhotonNetwork.player.customProperties["characterId"])) {
			item = new Item().get(characterItem);
			obj = getGameObject(characterItem._position);
			loadedPositions[characterItem._position] = true;

			if (!equipedItems.ContainsKey(characterItem._position)) {
				equipedItems[characterItem._position] = item;

				setItem(obj, item);
			} else {

				// check if user has changed its equipment
				if (equipedItems[characterItem._position].id != item.id) {
					equipedItems[characterItem._position] = item;

					// destroy current model
					unsetItem(obj);

					setItem(obj, item);
				}
			}

		}

		// if player has unequiped something, delete its view
		foreach (int pos in Enum.GetValues(typeof(CharacterItem.equipmentPosition))) {
			if (!loadedPositions.ContainsKey(pos)) {
				try {
					unsetItem(getGameObject(pos));
				} catch (Exception) {}
			}
		}

	}

	void setItem (GameObject obj, Item item) {
		GameObject newItem = (GameObject)Instantiate(Resources.Load(item.modelRoute), obj.transform.position, obj.transform.rotation);
		newItem.transform.SetParent(obj.transform);
		newItem.name = "Item";
	}

	void unsetItem (GameObject obj) {
		Destroy(obj.transform.Find("Item").gameObject);
	}

	GameObject getGameObject (int pos) {
		switch (pos) {
			case (int)CharacterItem.equipmentPosition.handLeft:
				return handLeft;
			case (int)CharacterItem.equipmentPosition.handRight:
				return handRight;
			default:
				throw new Exception("Position "+pos+" does not exist");
		}
	}
}
