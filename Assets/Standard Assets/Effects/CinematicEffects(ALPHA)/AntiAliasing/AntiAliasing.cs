using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Anti-aliasing")]
    public class AntiAliasing : MonoBehaviour
    {
        public enum Method
        {
            Smaa,
            Fxaa
        }

        [SerializeField]
        private SMAA m_SMAA = new SMAA();

        [SerializeField]
        private FXAA m_FXAA = new FXAA();

        [SerializeField, HideInInspector]
        private int m_Method = (int)Method.Smaa;
        public int method
        {
            get { return m_Method; }

            set
            {
                if (m_Method == value)
                    return;

                m_Method = value;
            }
        }

        public IAntiAliasing current
        {
            get
            {
                if (method == (int)Method.Smaa)
                    return m_SMAA;
                else
                    return m_FXAA;
            }
        }

        private Camera m_Camera;
        public Camera cameraComponent
        {
            get
            {
                if (m_Camera == null)
                    m_Camera = GetComponent<Camera>();

                return m_Camera;
            }
        }

        private void OnEnable()
        {
            m_SMAA.OnEnable(this);
            m_FXAA.OnEnable(this);
        }

        private void OnDisable()
        {
            m_SMAA.OnDisable();
            m_FXAA.OnDisable();
        }

        private void OnPreCull()
        {
            current.OnPreCull(cameraComponent);
        }

        private void OnPostRender()
        {
            current.OnPostRender(cameraComponent);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            current.OnRenderImage(cameraComponent, source, destination);
        }
    }
}
