using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

    private void Awake() {
        GetComponent<VideoPlayer>().loopPointReached += EndReached;
    }
	
	void Update () {
		if (Input.anyKeyDown) {
			SceneManager.LoadScene("MainMenu");
		}
	}

	void EndReached(VideoPlayer vp) {
        SceneManager.LoadScene("MainMenu");
	}
}
