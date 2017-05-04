using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainPanel : MonoBehaviour {
    public Locales myLocale;
    public Text nickLabel;
	public Text LevelLabel;
    public Toggle translateEN;
	public Button JoinButton;

	private int playerId;

	public void OnEnable () {
        myLocale = GetComponent<Locales>();
        bool hasPlayer = false;

		if (Service.db.SelectCount("FROM item") < 1) {
			DBSetup.start();
		}

        //NetworkManager.validateGameVersion();

        // @ToDo: create a UI for selection
        foreach (CharacterData character in Service.db.Select<CharacterData>("FROM characters")) {
			hasPlayer = true;
			playerId = character.id;

            if (PlayerPrefs.GetString("Locale") == "es_en")
                translateEN.isOn = true;
         
            nickLabel.text = character.name;
			LevelLabel.text = "Nivel " + character.level.ToString();
			JoinButton.onClick.AddListener(
				delegate {
				this.joinGame(character.id, character.name);
			});
			break;
		}
		if (hasPlayer) {
			nickLabel.transform.gameObject.SetActive(true);
			LevelLabel.transform.gameObject.SetActive(true);
			JoinButton.transform.gameObject.SetActive(true);
			#if UNITY_EDITOR
			GameObject.Find ("Canvas/MainPanel/LoginOptions/TestButton").SetActive(true);
			#endif

			GameObject.Find ("Canvas/MainPanel/LoginOptions/CreateButton").SetActive(false);
		} else {
			nickLabel.transform.gameObject.SetActive(false);
			LevelLabel.transform.gameObject.SetActive(false);
			JoinButton.transform.gameObject.SetActive(false);
			GameObject.Find ("Canvas/MainPanel/LoginOptions/TestButton").SetActive(false);
		}
		 
	}

    public void TranslationToggle() {
        if(translateEN.isOn) {
            PlayerPrefs.SetString("Locale", "es_en");
        } else {
            PlayerPrefs.SetString("Locale","es");
        }
        PlayerPrefs.Save();
    }

	public void joinGame (int characterId, string name) {

		if (characterId < 1) {
			return;
		}
        TranslationToggle();

        Hashtable h = new Hashtable(1);
		h.Add("characterId", characterId);

		PhotonNetwork.player.SetCustomProperties(h);
		PhotonNetwork.player.name = name;
		
		NetworkManager.Instance.startConnection();
		GameObject.Find ("Canvas/MainPanel/LoginOptions/JoinButton/Text").GetComponent<Text> ().text = "Conectando...";
	}

	public void joinTest () {
		Menu.defaultLevel = Menu.debugLevel;
		joinGame (playerId, "Tester");
	}
}
