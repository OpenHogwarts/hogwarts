using UnityEngine;
using System.Collections;
using iBoxDB.LocalServer;

public class NetworkManager : Photon.MonoBehaviour {

	public Texture mmarow;
	public static NetworkManager Instance;
	
	void Start () {
		Instance = this;
	}

	public void startConnection () {
		PhotonNetwork.ConnectUsingSettings (Menu.GAME_VERSION);
	}
	
	public void spawnPlayer ()
	{
		GameObject player = PhotonNetwork.Instantiate("Characters/Player", GameObject.Find("SpawnPoints/FirstJoin").transform.position, Quaternion.identity, 0);

		// get character data
		CharacterData character = Service.db.SelectKey<CharacterData> ("characters", PhotonNetwork.player.customProperties["characterId"]);
		player.GetComponent<Player> ().characterData = character;
		
		player.GetComponent<Motor> ().enabled = true;
		player.GetComponent<PlayerHotkeys> ().enabled = true;
		player.GetComponent<PlayerCombat> ().enabled = true;
		player.transform.FindChild ("Main Camera").gameObject.SetActive(true);
		player.transform.FindChild ("NamePlate").gameObject.SetActive(false);

		// Set minimap target
		GameObject.Find("MiniMapCamera").GetComponent<MiniMap>().target = player.transform;
		GameObject.Find("MiniMapElementsCamera").GetComponent<MiniMap>().target = player.transform;

		player.transform.FindChild ("Indicator").GetComponent<Renderer>().material.mainTexture = mmarow;
	}
	/*
	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		chat.sendMessage(player.name + " left the game");
	}

	void OnPhotonPlayerConnect(PhotonPlayer player)
	{
		chat.sendMessage(player.name + " joined the game");
	}*/

	void OnJoinedLobby () {
		PhotonNetwork.LoadLevel(Menu.defaultLevel);
		PhotonNetwork.JoinRandomRoom();
		Menu.Instance.showPanel("LoadingPanel");
	}
	
	void OnJoinedRoom()
	{
		spawnPlayer();
	}
	
	void OnCreatedRoom()
	{
		//OnJoinedRoom ();
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
