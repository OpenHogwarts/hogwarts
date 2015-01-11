using UnityEngine;
using System.Collections;

/*
	This class is used to setup DB items
 **/

public class DBSetup : MonoBehaviour {

	public static void insertItems () {
		ItemData item = new ItemData ();
		item.id = 1;
		item.name = "Queso de cabra";
		item.description = "Hecho en el mejor caserío muggle";
		item.type = Item.ItemType.Consumable;
		item.subType = Item.ItemSubType.Health;
		item.create ();
		
		
		ItemData item2 = new ItemData ();
		item2.id = 2;
		item2.name = "Colgante de Dermor";
		item2.description = "No se me ocurre nada bueno";
		item2.type = Item.ItemType.Armor;
		item2.subType = Item.ItemSubType.Necklace;
		item2.create ();
	}

	public static void insertTestItems () {
		CharacterItem item = new CharacterItem ();
		item.id = 1;
		item.item = 1;
		item.character = 1;
		item.quantity = 2;
		item.create();
	}
}
