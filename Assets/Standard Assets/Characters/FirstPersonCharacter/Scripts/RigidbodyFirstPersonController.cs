using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        public AdvancedSettings advancedSettings = new();


        public Camera cam;
        private CapsuleCollider m_Capsule;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded;


        private Rigidbody m_RigidBody;
        private float m_YRotation;
        public MouseLook mouseLook = new();
        public MovementSettings movementSettings = new();


        public Vector3 Velocity => m_RigidBody.velocity;

        public bool Grounded { get; private set; }

        public bool Jumping { get; private set; }

        public bool Running
        {
            get
            {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
	            return false;
#endif
            }
        }


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init(transform, cam.transform);
        }


        private void Update()
        {
            RotateView();

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump) m_Jump = true;
        }


        private void FixedUpdate()
        {
            GroundCheck();
            var input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) &&
                (advancedSettings.airControl || Grounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                var desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
            }

            if (Grounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    Jumping = true;
                }

                if (!Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon &&
                    m_RigidBody.velocity.magnitude < 1f) m_RigidBody.Sleep();
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !Jumping) StickToGroundHelper();
            }

            m_Jump = false;
        }


        private float SlopeMultiplier()
        {
            var angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset),
                    Vector3.down, out hitInfo,
                    m_Capsule.height / 2f - m_Capsule.radius +
                    advancedSettings.stickToGroundHelperDistance, ~0, QueryTriggerInteraction.Ignore))
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
        }


        private Vector2 GetInput()
        {
            var input = new Vector2
            {
                x = CrossPlatformInputManager.GetAxis("Horizontal"),
                y = CrossPlatformInputManager.GetAxis("Vertical")
            };
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            var oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

            if (Grounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                var velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = Grounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset),
                    Vector3.down, out hitInfo,
                    m_Capsule.height / 2f - m_Capsule.radius + advancedSettings.groundCheckDistance, ~0,
                    QueryTriggerInteraction.Ignore))
            {
                Grounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                Grounded = false;
                m_GroundContactNormal = Vector3.up;
            }

            if (!m_PreviouslyGrounded && Grounded && Jumping) Jumping = false;
        }

        [Serializable]
        public class MovementSettings
        {
            public float BackwardSpeed = 4.0f; // Speed when walking backwards
            [HideInInspector] public float CurrentTargetSpeed = 8f;
            public float ForwardSpeed = 8.0f; // Speed when walking forward
            public float JumpForce = 30f;

#if !MOBILE_INPUT
#endif
            public KeyCode RunKey = KeyCode.LeftShift;
            public float RunMultiplier = 2.0f; // Speed when sprinting

            public AnimationCurve SlopeCurveModifier = new(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f),
                new Keyframe(90.0f, 0.0f));

            public float StrafeSpeed = 4.0f; // Speed when walking sideways

#if !MOBILE_INPUT
            public bool Running { get; private set; }
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;
                if (input.x > 0 || input.x < 0)
                    //strafe
                    CurrentTargetSpeed = StrafeSpeed;
                if (input.y < 0)
                    //backwards
                    CurrentTargetSpeed = BackwardSpeed;
                if (input.y > 0)
                    //forwards
                    //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                    CurrentTargetSpeed = ForwardSpeed;
#if !MOBILE_INPUT
                if (Input.GetKey(RunKey))
                {
                    CurrentTargetSpeed *= RunMultiplier;
                    Running = true;
                }
                else
                {
                    Running = false;
                }
#endif
            }
        }


        [Serializable]
        public class AdvancedSettings
        {
            public bool airControl; // can the user control the direction that is being moved in the air

            public float
                groundCheckDistance =
                    0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )

            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float
                shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)

            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public float stickToGroundHelperDistance = 0.5f; // stops the character
        }
    }
}