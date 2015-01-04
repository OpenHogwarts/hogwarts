
#pragma warning disable 1587
/// \file
/// <summary>Implements a RPC Attribute for platforms that don't have it in UnityEngine.</summary>
#pragma warning restore 1587

#if !UNITY_EDITOR && (UNITY_WINRT || UNITY_WP8 || UNITY_PS3 || UNITY_WIIU)

using System;


namespace UnityEngine
{
    public class RPC : Attribute
    {
    }
}
#endif
