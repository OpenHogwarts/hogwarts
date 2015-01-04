using UnityEngine;
using System.Collections;

/// <summary>
/// This component makes it easy to switch scenes or open webpages on click.
/// Requires a InputToEvent component on the camera to forward clicks on screen.
/// </summary>
public class OnClickLoadSomething : MonoBehaviour
{
    public enum  ResourceTypeOption : byte
    {
        Scene,
        Web
    }

    public ResourceTypeOption ResourceTypeToLoad = ResourceTypeOption.Scene;
    public string ResourceToLoad;

    public void OnClick()
    {
        switch (ResourceTypeToLoad)
        {
            case ResourceTypeOption.Scene:
                Application.LoadLevel(ResourceToLoad);
                break;
            case ResourceTypeOption.Web:
                Application.OpenURL(ResourceToLoad);
                break;
        }
    }
}
