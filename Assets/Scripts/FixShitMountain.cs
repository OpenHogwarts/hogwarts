using UnityEngine;

public static class FixShitMountain
{
    public static T GetInstanceFromScene<T>() where T : Object
    {
        return Object.FindObjectOfType<T>();
    }
}