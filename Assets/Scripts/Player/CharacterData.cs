public class CharacterData {
	
	public int id;
	public string name;
	public string model;
	public string position;
	public int level;
	
	public int health;
	public int maxHealth;
	public int mana;
	public int maxMana;
	public int exp;
	
	public int knut;
	public int sickle;
	public int galleon;

	public void save () {
		Menu.db.Update ("characters", this);
	}
}
