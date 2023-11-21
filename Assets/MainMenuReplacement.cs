using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// A Replacement for main panel
/// </summary>
public class MainMenuReplacement : MonoBehaviour
{

    private void joinGame(int characterId, string name)
    {

        if (characterId < 1)
        {
            return;
        }

        ExitGames.Client.Photon.Hashtable h = new Hashtable(1) { { "characterId", characterId } };

        PhotonNetwork.player.SetCustomProperties(h);
        PhotonNetwork.player.NickName = name;
        this.gameObject.GetComponent<NetworkManager>().startConnection();
        //GameObject.Find("Canvas/MainPanel/LoginOptions/JoinButton/Text").GetComponent<Text>().text = LanguageManager.get("CONNECTING") + "...";
    }
    private void OnEnable()
    {



    }
    // Start is called before the first frame update
    void Start()
    {

        if (Service.db.SelectCount("FROM item") < 1)
        {
            DBSetup.start();
        }

        // @ToDo: create a UI for selection
        var character = Service.db.Select<CharacterData>("FROM characters").FirstOrDefault();
        if (character is null)
        {
            const string nick = "Harry Potter";
            const int initialHealth = 270;
            const int initialMana = 130;

            if (!new CharacterData
                {
                    name = nick,
                    model = "male_01",
                    position = "",
                    level = 1,
                    health = initialHealth,
                    maxHealth = initialHealth,
                    mana = initialMana,
                    maxMana = initialMana,
                    money = 234670,
                    id = Service.db.Id(1)
                }.create())
            {
                Debug.Log("Failed to create character.");
                return;
            }

        }


        this.joinGame(1, character.name);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
