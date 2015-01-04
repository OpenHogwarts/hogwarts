using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainPanel : MonoBehaviour {

	private string defaultLevel = "Hogwarts";
	private string debugLevel = "Test";
	
	void Start () {
		GameObject.Find ("Canvas/MainPanel/NickInput/Text").GetComponent<Text> ().text = PhotonNetwork.player.name;
	}

	void updateNick () {
		string nick = GameObject.Find ("Canvas/MainPanel/NickInput/Text").GetComponent<Text> ().text;
		if (nick == "") { return; }

		PlayerPrefs.SetString ("Nick", nick);
		PhotonNetwork.player.name = nick;
	}

	public void joinGame () {
		updateNick ();
		PhotonNetwork.ConnectUsingSettings( Menu.GAME_VERSION );

		GameObject.Find ("Canvas/MainPanel/JoinButton/Text").GetComponent<Text> ().text = "Connecting...";
	}

	public void joinTest () {
		defaultLevel = debugLevel;
		joinGame ();
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
