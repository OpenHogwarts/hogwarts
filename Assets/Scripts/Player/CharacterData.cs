public class CharacterData {

	protected string TABLE_NAME = "characters";
	
	public int id;
	public string name;
	public string model;
	public string position;
	public int level;
	public int house;
	
	public int health;
	public int maxHealth;
	public int mana;
	public int maxMana;
	public int exp;
	
	public int money;

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}

	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
