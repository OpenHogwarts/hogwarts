using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Item : ItemData
{
	public CharacterItem characterItem;
	Texture _icon;

	// Load the resource only if is needed
	public Texture icon
	{
		get {
			if (!_icon) {
				_icon = Resources.Load<Texture>("2DTextures/Items/item"+id.ToString());
			}

			return _icon;
		}
	}

	string _model;
	
	/* Having
		id: 4
		type: Weapon
		subtype: Wand

		We will get Items/Weapon/Wand/Wand_01/Wand_01
	*/
	public string modelRoute
	{
		get {
			if (_model == null) {
				string nId;
				string mName;

				if (id < 10) {
					nId = "0"+id;
				} else {
					nId = id.ToString();
				}
				mName = ((ItemSubType)subType).ToString() + "_"+ nId;

				_model = "Items/";
				_model += ((ItemType)type).ToString() + "/";
				_model += ((ItemSubType)subType).ToString() + "/";
				_model += mName + "/" + mName;
			}
			
			return _model;
		}
	}

	public int quantity
	{
		get {return characterItem.quantity;}
		set {
			characterItem.quantity = value;
			characterItem.save();
		}
	}

	public Item get (CharacterItem cItem) {
		Item itm = Item.get(cItem.item);
		itm.characterItem = cItem;
		return itm;
	}

	public static Item get (int id) {
		return Service.db.SelectKey<Item> ("item", id);
	}

	/**
	 * Use this item
	 *
	 */
	public void use () {
		switch (type) {
			case Item.ItemType.Consumable:
				switch(subType) {
					case Item.ItemSubType.Health:
						Player.Instance.health += health;
						characterItem.quantity--;
					break;
				}
				break;
			default:
				return;
		}
		characterItem.save ();
	}

	/**
	 * Validate if this item can be set on the select equipment position
	 *
	 */
	public bool isValidEquipmentPosition (CharacterItem.equipmentPosition pos) {
		switch (type) {
			case ItemType.Weapon:
					if (pos == CharacterItem.equipmentPosition.handLeft || pos == CharacterItem.equipmentPosition.handRight) {
						return true;
					}
				break;
			case ItemType.Armor:
				switch (subType) {
					case ItemSubType.Head:
							if (pos == CharacterItem.equipmentPosition.head) {
								return true;
							}
						break;
						case ItemSubType.Hands:
							if (pos == CharacterItem.equipmentPosition.hands) {
								return true;
							}
						break;
				}
				break;
			case ItemType.Consumable:
			case ItemType.Container:
			case ItemType.Useless:
				return false;
		}
		return false;
	}
}