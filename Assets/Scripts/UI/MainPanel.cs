using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainPanel : MonoBehaviour {

	private string defaultLevel = "Hogwarts";
	private string debugLevel = "Test";

	public void Start () {
		bool hasPlayer = false;

		if (Service.db.SelectCount("FROM item") < 1) {
			DBSetup.insertItems();
			DBSetup.insertTestItems();
		}
		

		// @ToDo: create a UI for selection
		foreach (CharacterData character in Service.db.Select<CharacterData>("FROM characters")) {
			hasPlayer = true;

			GameObject.Find ("Canvas/MainPanel/CharacterPanel/NickLabel").GetComponent<Text>().text = character.name;
			GameObject.Find ("Canvas/MainPanel/CharacterPanel/LevelLabel").GetComponent<Text>().text = character.level.ToString();
			GameObject.Find ("Canvas/MainPanel/CharacterPanel/JoinButton").GetComponent<Button>().onClick.AddListener(
				delegate {
				this.joinGame(character.id, character.name);
			});
			break;
		}
		if (hasPlayer) {
			GameObject.Find ("Canvas/MainPanel/CreateButton").SetActive(false);
		} else {
			GameObject.Find ("Canvas/MainPanel/CharacterPanel/NickLabel").SetActive(false);
			GameObject.Find ("Canvas/MainPanel/CharacterPanel/LevelLabel").SetActive(false);
			GameObject.Find ("Canvas/MainPanel/CharacterPanel/JoinButton").SetActive(false);
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
		
		PhotonNetwork.ConnectUsingSettings (Menu.GAME_VERSION);
		GameObject.Find ("Canvas/MainPanel/CharacterPanel/JoinButton/Text").GetComponent<Text> ().text = "Conectando...";
	}

	public void joinTest () {
		defaultLevel = debugLevel;
		joinGame (1, "Tester");
	}

	void OnJoinedLobby () {
		PhotonNetwork.JoinRandomRoom();
	}

	void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel(defaultLevel);
	}

	void OnCreatedRoom()
	{
		PhotonNetwork.LoadLevel(defaultLevel);
	}

	void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null);
	}


	public void OnPhotonCreateRoomFailed()
	{

		Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
	}
	
	public void OnPhotonJoinRoomFailed()
	{

		Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
	}

	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("Disconnected from Photon.");
	}
	
	public void OnFailedToConnectToPhoton(object parameters)
	{

		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
	}
}
