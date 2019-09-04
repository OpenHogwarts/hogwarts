using UnityEngine;
using System.Collections;
using iBoxDB.LocalServer;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class NetworkManager : Photon.MonoBehaviour {

	public Texture mmarow;
	public static NetworkManager Instance;
	
	void Start () {
		Instance = this;
	}

    public static void validateGameVersion ()
    {
        // http://answers.unity3d.com/questions/792342/how-to-validate-ssl-certificates-when-using-httpwe.html
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        string latestVersion = (new System.Net.WebClient()).DownloadString("https://raw.githubusercontent.com/OpenHogwarts/hogwarts/master/latest_build.txt").Trim();

        if (Menu.GAME_VERSION != latestVersion) {
            Application.Quit();
            throw new System.Exception("Please download the latest build " + Menu.GAME_VERSION + " <-> " + latestVersion);
        }
    }

	public void startConnection () {
        PhotonNetwork.ConnectUsingSettings(Menu.GAME_VERSION);
	}
	
	public void spawnPlayer ()
	{
		GameObject player = PhotonNetwork.Instantiate("Characters/Player", GameObject.Find("SpawnPoints/FirstJoin").transform.position, Quaternion.identity, 0);

		// get character data
		CharacterData character = Service.db.SelectKey<CharacterData> ("characters", PhotonNetwork.player.customProperties["characterId"]);
		player.GetComponent<Player> ().characterData = character;

		player.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>().enabled = true;
		player.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().enabled = true;
		//player.GetComponent<Motor> ().enabled = true;
		player.GetComponent<PlayerHotkeys> ().enabled = true;
		player.GetComponent<PlayerCombat> ().enabled = true;
		player.transform.Find ("Main Camera").gameObject.SetActive(true);
		player.transform.Find ("NamePlate").gameObject.SetActive(false);

		// Set minimap target
		GameObject.Find("MiniMapCamera").GetComponent<MiniMap>().target = player.transform;
		GameObject.Find("MiniMapElementsCamera").GetComponent<MiniMap>().target = player.transform;

		GameObject.Find ("Canvas/TopMenu/Config").GetComponent<ConfigMenu> ().player = player;

		player.transform.Find ("Indicator").GetComponent<Renderer>().material.mainTexture = mmarow;
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

    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
}
