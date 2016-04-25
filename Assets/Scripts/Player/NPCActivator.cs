using UnityEngine;
using System.Collections;

/*
Collides with NPCs (using a FOV width sphere) to enable/disable them
*/

public class NPCActivator : MonoBehaviour
{

    void OnTriggerStay(Collider col)
    {
        if (col.tag != "NPC" || col.isTrigger) {
            return;
        }

        // get the ownership so NPC can move
        if (col.gameObject.GetComponent<PhotonView>().owner == null) {
            col.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player);
        }

        col.gameObject.GetComponent<NPC>().setEnabled(true);
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag != "NPC" || col.isTrigger) {
            return;
        }

        if (col.gameObject.GetComponent<PhotonView>().isMine) {
            col.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.masterClient);
        }

        col.gameObject.GetComponent<NPC>().setEnabled(false);
    }
}
