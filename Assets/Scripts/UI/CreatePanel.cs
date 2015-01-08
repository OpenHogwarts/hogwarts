using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreatePanel : MonoBehaviour {

	public void createButton () {
		string nick = GameObject.Find ("Canvas/CreatePanel/NickInput/Text").GetComponent<Text> ().text;

		if (nick.Length < 3 || Menu.db.SelectCount ("FROM characters WHERE name = ?", nick) != 0) {
			return;
		}

		int inicialHealth = 270;
		int inicialMana = 130;

		var data = new CharacterData
		{
			name = nick,
			model = "male_01",
			position = "",
			level = 1,
			health = inicialHealth,
			maxHealth = inicialHealth,
			mana = inicialMana,
			maxMana = inicialMana,
			knut = 50,
			sickle = 0,
			galleon = 0,
			id = Menu.db.Id(1)
		};
		bool sucess = Menu.db.Insert ("characters", data);

		if (sucess) {
			Application.LoadLevel(Application.loadedLevel);
		} else {
			Debug.Log("Something went wrong");
		}
	}
}
