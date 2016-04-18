using UnityEngine;
using System.Collections;

public class NPCManager : MonoBehaviour {

    public static NPCManager Instance;

	public void Start () {
        Instance = this;
    }

    public void prepareRespawn (NPC npc) {
        StartCoroutine(respawn(npc));
    }

    public IEnumerator respawn (NPC npc)
    {
        // wait before disappear so players can loot it.
        yield return new WaitForSeconds(10f);

        npc.namePlate.gameObject.SetActive(false);
        npc.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        npc.namePlate.gameObject.SetActive(true);
        npc.gameObject.SetActive(true);
        npc.reset();
    }
}
