using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    public static class ImageEffectHelper
    {
        public static bool IsSupported(Shader s, bool needDepth, bool needHdr, MonoBehaviour effect)
        {
            if (s == null || !s.isSupported)
            {
                Debug.LogWarningFormat("Missing shader for image effect {0}", effect);
                return false;
            }

            if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
                return false;

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
                return false;

            if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                return false;

            return true;
        }

        public static Material CheckShaderAndCreateMaterial(Shader s)
        {
            if (s == null || !s.isSupported)
                return null;

            var material = new Material(s);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        public static bool supportsDX11
        {
            get { return SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders; }
        }
    }
}
