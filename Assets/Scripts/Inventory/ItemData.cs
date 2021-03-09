
public class ItemData {

	protected string TABLE_NAME = "item";

	public int id;
	public string name {
        get {
            return LanguageManager.get("ITEM_" + id + "_NAME");
        }
    }
	public string description {
        get {
            return LanguageManager.get("ITEM_" + id + "_DESCRIPTION");
        }
    }
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
		Armor = 1,
		Weapon = 2,
		Consumable = 3,
		Useless = 4,
		Container = 5
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

		// **  WEAPON ** //
		Wand = 9,
		Staff = 10,
		Sword = 11,
		Pistol = 12,
		
		// **  CONSUMABLE ** //
		Health = 13,
		Mana = 14,

		// ** USELESS **//
		Quest = 15,
		Scrap = 16
		
	}

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
