using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraCollisionSpeed = 2f;

    public Transform cameraTarget;

    public float cameraTargetHeight = 1.0f;
    private float correctedDistance;
    private float currentDistance;
    public float desiredDistance;
    private float distance = 6;
    private bool isHitting;
    public float lastDistance;
    private readonly int lerpRate = 5;

    public float maxViewDistance = 25;
    public float minViewDistance = 1;

    private readonly int mouseXSpeedMod = 5;
    private readonly int mouseYSpeedMod = 3;
    private float oldDistance;
    private bool reachedDist = true;
    private float x;
    private float y;
    public int zoomRate = 30;

    private void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.x;
        y = angles.y;
        distance = PlayerPrefs.GetFloat("CameraDistance", distance);
        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;
    }

    private void LateUpdate()
    {
        if (GamePanel.isMovingAPanel) return;

        if (InputSystemAgent.GetKey("RMaus"))
        {

            x += InputSystemAgent.ViewMove.x;
            y -= InputSystemAgent.ViewMove.y;

            if (InputSystemAgent.NormalMove.x != 0 || InputSystemAgent.NormalMove.y != 0)
            {
                var targetRotationAngle = cameraTarget.eulerAngles.y;
                var cameraRotationAngle = transform.eulerAngles.y;
                x = Mathf.LerpAngle(cameraRotationAngle, targetRotationAngle, lerpRate * Time.deltaTime);
            }
        }



        y = ClampAngle(y, -50, 80);

        var rotation = Quaternion.Euler(y, x, 0);
        var isd=InputSystemAgent.DesiredDistance;
        desiredDistance -= isd/1200
             * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);
        correctedDistance = desiredDistance;
        currentDistance = correctedDistance;

        var position = 
            cameraTarget.position -
                   (rotation * Vector3.forward * currentDistance + new Vector3(0, -cameraTargetHeight, 0));
        
        transform.rotation = rotation;
        transform.position = position;

        if (isHitting)
        {
            desiredDistance -= 0.01f * (Time.deltaTime * cameraCollisionSpeed) * zoomRate * Mathf.Abs(desiredDistance);
            desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);
        }
        else
        {
            Debug.DrawLine(transform.position - transform.forward * 0.5f,
                transform.position - transform.forward * (lastDistance - desiredDistance));
            if (desiredDistance < lastDistance)
            {
                if (!Physics.Raycast(transform.position - transform.forward * 0.5f, -transform.forward,
                        lastDistance - desiredDistance) && !Physics.Raycast(
                        transform.position - transform.forward * (lastDistance - desiredDistance + 0.5f), Vector3.down,
                        0.5f))
                {
                    desiredDistance += 0.01f * (Time.deltaTime * cameraCollisionSpeed) * zoomRate *
                                       Mathf.Abs(desiredDistance);
                    desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);
                }
            }
            else
            {
                reachedDist = true;
                lastDistance = 0;
            }
        }
    }

    // set play camera preferences before quit
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("CameraDistance", currentDistance);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!col.isTrigger)
        {
            isHitting = true;
            if (reachedDist)
            {
                lastDistance = currentDistance;
                reachedDist = false;
            }
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (!col.isTrigger) isHitting = true;
    }

    private void OnTriggerExit(Collider col)
    {
        if (!col.isTrigger) isHitting = false;
    }
}