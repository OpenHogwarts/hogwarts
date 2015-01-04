using UnityEngine;
using System.Collections;

public class NetworkManager : Photon.MonoBehaviour {

	public void Awake()
	{
		// in case we started this demo with the wrong scene being active, simply load the menu scene
		if (!PhotonNetwork.connected)
		{
			Application.LoadLevel("Menu");
			return;
		}
		
		GameObject player = PhotonNetwork.Instantiate("Player", GameObject.Find("SpawnPoints/FirstJoin").transform.position, Quaternion.identity, 0);

		player.GetComponent<MouseLook> ().enabled = true;
		player.GetComponent<CharacterController>().enabled = true;
		player.GetComponent<PlayerMovement> ().enabled = true;
		player.transform.FindChild ("Main Camera").gameObject.SetActive(true);
		player.transform.FindChild ("NamePlate").GetComponent<TextMesh>().text = PhotonNetwork.playerName;
		player.transform.FindChild ("NamePlate").gameObject.SetActive(false);
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Chat chat = GetComponent<Chat>();
		chat.sendMessage(player.name + " left the game");
	}

	void OnPhotonPlayerConnect(PhotonPlayer player)
	{
		Chat chat = GetComponent<Chat>();
		chat.sendMessage(player.name + " joined the game");
	}
}
