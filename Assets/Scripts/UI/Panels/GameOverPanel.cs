using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPanel : MonoBehaviour {

    public GameObject respawnButton;


    public void OnRespawn()
    {
        Player.Instance.Reborn();
    }
}
