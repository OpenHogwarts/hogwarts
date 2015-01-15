public class ItemData {

	protected string TABLE_NAME = "item";

	public int id;
	public string name;
	public string description;
	public ItemType type;
	public ItemSubType subType;

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
		Head,
		Boots,
		Chest,
		Pants,
		Earring,
		Necklace,
		Ring,
		Hands,
		
		// **  CONSUMABLE ** //
		Health,
		Mana,

		// ** USELESS **//
		Quest,
		Scrap
		
	}

	public void save () {
		Menu.db.Update (TABLE_NAME, this);
	}
	
	public bool create () {
		return Menu.db.Insert (TABLE_NAME, this);
	}
}
