using UnityEngine;

public class BroomstickControl : MonoBehaviour
{
    public float curSpeed;
    public float damping = 1f;
    public float latSpeed;
    public float maxSpeed = 10.0f;

    public new Rigidbody rigidbody;
    private Vector3 rotation;
    private Vector3 targetVelocity;
    public float upSpeed;

    // Use this for initialization
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {

        curSpeed = Mathf.Lerp(curSpeed, InputSystemAgent.FlyMove.z * maxSpeed, 0.1f);
        upSpeed = Mathf.Lerp(upSpeed, InputSystemAgent.FlyMove.y * maxSpeed, 0.1f);

        latSpeed = Mathf.Lerp(latSpeed, InputSystemAgent.FlyMove.x * maxSpeed, 0.1f);


        targetVelocity = new Vector3(latSpeed, upSpeed, curSpeed);
        targetVelocity = Camera.main.transform.TransformDirection(targetVelocity);


        rigidbody.velocity = targetVelocity;
    }

    private void Update()
    {
        var rotation = Quaternion.LookRotation(transform.position -
                                               Vector3.Scale(Camera.main.transform.position, new Vector3(1, 0.99f, 1)));
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
    }
}