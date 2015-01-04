using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	public GameObject OptionsPanel;
	public GameObject MainPanel;

	private static string defaultNick = "Wannabe";
	public const string GAME_VERSION = "0.01";

	void Start () {
		PhotonNetwork.player.name = PlayerPrefs.GetString("Nick", defaultNick);
	}
}