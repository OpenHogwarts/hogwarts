using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public enum Effect {
		Buy,
        QuestComplete,
        QuestAccept
    }

	public static SoundManager Instance;

	public void Start () {
		Instance = this;
	}

	public static AudioClip get (Effect key) {
		return Resources.Load("Sound/" + System.Enum.GetName(typeof(Effect), key)) as AudioClip;
	}
}
