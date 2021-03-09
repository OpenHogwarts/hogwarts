using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour {

    private string id;

    private void Awake() {
        id = GetComponent<Text>().text;
    }

    private void Start() {
        reload();
    }

    public void reload () {
        GetComponent<Text>().text = LanguageManager.get(id);
    }
}
