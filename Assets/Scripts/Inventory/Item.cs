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

	public int quantity
	{
		get {return characterItem.quantity;}
		set {
			characterItem.quantity = value;
			characterItem.save();
		}
	}

	public Item  get (CharacterItem cItem) {
		foreach (Item itm in Menu.db.Select<Item>("FROM item WHERE id = ?", cItem.item)) {
			itm.characterItem = cItem;
			return itm;
			break;
		}
		throw new Exception("item not found");
	}
}