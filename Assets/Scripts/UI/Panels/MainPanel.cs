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
			GameObject.Find ("Canvas/MainPanel/CreateButton").SetActive(false);
		} else {
			nickLabel.transform.gameObject.SetActive(false);
			LevelLabel.transform.gameObject.SetActive(false);
			JoinButton.transform.gameObject.SetActive(false);
			GameObject.Find ("Canvas/MainPanel/TestButton").SetActive(false);
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
		GameObject.Find ("Canvas/MainPanel/CharacterPanel/JoinButton/Text").GetComponent<Text> ().text = "Conectando...";
	}

	public void joinTest () {
		Menu.defaultLevel = Menu.debugLevel;
		joinGame (playerId, "Tester");
	}
}
