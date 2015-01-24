using System;
public class ItemData {

	protected string TABLE_NAME = "item";

	public int id;
	public string name;
	public string description;
	public int _type;
	public int _subType;
	public ItemType type {
		get {
			return (ItemType)_type;
		}
		set {
			_type = (int)value;
		}
	}
	public ItemSubType subType {
		get {
			return (ItemSubType)_subType;
		}
		set {
			_subType = (int)value;
		}
	}
	public int price;

	// CUSTOM ATTRIBUTES BASED ON ITEM TYPE
	public int health;

	public enum ItemType
	{
		Armor,
		Weapon,
		Consumable,
		Useless,
		Container
	}
	
	public enum ItemSubType
	{
		// **  ARMOR ** //
		Head = 1,
		Boots = 2,
		Chest = 3,
		Pants = 4,
		Earring = 5,
		Necklace = 6,
		Ring = 7,
		Hands = 8,
		
		// **  CONSUMABLE ** //
		Health,
		Mana,

		// ** USELESS **//
		Quest,
		Scrap
		
	}

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
