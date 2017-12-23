using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainPanel : MonoBehaviour {
    public Text nickLabel;
	public Text LevelLabel;
	public Button JoinButton;

	private int playerId;

	public void OnEnable () {
        bool hasPlayer = false;

		if (Service.db.SelectCount("FROM item") < 1) {
			DBSetup.start();
		}

        //NetworkManager.validateGameVersion();

        // @ToDo: create a UI for selection
        foreach (CharacterData character in Service.db.Select<CharacterData>("FROM characters")) {
			hasPlayer = true;
			playerId = character.id;
         
            nickLabel.text = character.name;
			LevelLabel.text = character.level.ToString();
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

	public void joinGame (int characterId, string name) {

		if (characterId < 1) {
			return;
		}

        Hashtable h = new Hashtable(1);
		h.Add("characterId", characterId);

		PhotonNetwork.player.SetCustomProperties(h);
		PhotonNetwork.player.name = name;
		
		NetworkManager.Instance.startConnection();
		GameObject.Find ("Canvas/MainPanel/LoginOptions/JoinButton/Text").GetComponent<Text> ().text = LanguageManager.get("CONNECTING") + "...";
	}

	public void joinTest () {
		Menu.defaultLevel = Menu.debugLevel;
		joinGame (playerId, "Tester");
	}
}
