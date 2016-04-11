using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    public interface IAntiAliasing
    {
        void OnEnable(AntiAliasing owner);
        void OnDisable();
        void OnPreCull(Camera camera);
        void OnPostRender(Camera camera);
        void OnRenderImage(Camera camera, RenderTexture source, RenderTexture destination);
    }
}
