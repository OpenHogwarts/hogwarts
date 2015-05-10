using UnityEngine;
using System.Collections;
using iBoxDB.LocalServer;

public class NetworkManager : Photon.MonoBehaviour {

	public Texture mmarow;
	
	public void Awake()
	{
		// in case we started with the wrong scene being active, simply load the menu scene
		if (!PhotonNetwork.connected)
		{
			Application.LoadLevel("MainMenu");
			return;
		}
		
		GameObject player = PhotonNetwork.Instantiate("Characters/Player", GameObject.Find("SpawnPoints/FirstJoin").transform.position, Quaternion.identity, 0);

		// get character data
		CharacterData character = Service.db.SelectKey<CharacterData> ("characters", PhotonNetwork.player.customProperties["characterId"]);
		player.GetComponent<Player> ().characterData = character;
		
		player.GetComponent<Motor> ().enabled = true;
		player.transform.FindChild ("Main Camera").gameObject.SetActive(true);
		player.transform.FindChild ("NamePlate").gameObject.SetActive(false);
		GameObject.Find("MiniMapCamera").GetComponent<MiniMap>().target = player.transform;
		GameObject.Find("MiniMapElementsCamera").GetComponent<MiniMap>().target = player.transform;
		player.transform.FindChild ("Indicator").renderer.material.mainTexture = mmarow;
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
}
