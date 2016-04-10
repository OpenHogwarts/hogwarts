using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    public class RenderTexureUtility
    {
        //Temporary render texture handling
        private List<RenderTexture> m_TemporaryRTs = new List<RenderTexture>();

        public RenderTexture GetTemporaryRenderTexture(int width, int height, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.ARGBHalf, FilterMode filterMode = FilterMode.Bilinear)
        {
            var rt = RenderTexture.GetTemporary(width, height, depthBuffer, format);
            rt.filterMode = filterMode;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.name = "RenderTextureUtilityTempTexture";
            m_TemporaryRTs.Add(rt);
            return rt;
        }

        public void ReleaseTemporaryRenderTexture(RenderTexture rt)
        {
            if (rt == null)
                return;

            if (!m_TemporaryRTs.Contains(rt))
            {
                Debug.LogErrorFormat("Attempting to remove texture that was not allocated: {0}", rt);
                return;
            }

            m_TemporaryRTs.Remove(rt);
            RenderTexture.ReleaseTemporary(rt);
        }

        public void ReleaseAllTemporyRenderTexutres()
        {
            for (int i = 0; i < m_TemporaryRTs.Count; ++i)
                RenderTexture.ReleaseTemporary(m_TemporaryRTs[i]);

            m_TemporaryRTs.Clear();
        }
    }
}
