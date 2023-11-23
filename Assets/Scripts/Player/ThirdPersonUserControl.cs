using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private Transform m_Cam; // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward; // The current forward direction of the camera
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object

        private bool
            m_Jump; // the world-relative desired move direction, calculated from the camForward and user input.

        private Vector3 m_Move;

        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
                m_Cam = Camera.main.transform;
            else
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
            m_CamForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
        }


        private void Update()
        {
            if (Chat.Instance.isWritting) return;

            if (!m_Jump) m_Jump = InputSystemAgent.GetKeyDown("Space");
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            if (Chat.Instance.isWritting) return;
            var mov=InputSystemAgent.NormalMove;
            var crouch = InputSystemAgent.GetKey("LCtrl");
            var h = mov.x;
            var v = mov.y;
            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                if (h != 0) m_CamForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

                m_Move = v * m_CamForward + h * transform.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
            // walk speed multiplier
            if (InputSystemAgent.GetKey("LShift")) m_Move *= 0.5f;

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }
    }
}