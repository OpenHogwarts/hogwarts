using UnityEngine;

/// <summary>
/// Utility component to forward mouse or touch input to clicked gameobjects.
/// Calls OnPress, OnClick and OnRelease methods on "first" game object.
/// </summary>
public class InputToEvent : MonoBehaviour
{

    private GameObject lastGo;
    public static Vector3 inputHitPos;
    public bool DetectPointedAtGameObject;
    public static GameObject goPointedAt { get; private set; }

    private Vector2 pressedPosition = Vector2.zero;
    private Vector2 currentPos = Vector2.zero;
    public bool Dragging;

    private Camera m_Camera;

    public Vector2 DragVector
    {
        get { return this.Dragging ? this.currentPos - pressedPosition : Vector2.zero; }
    }

    void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if( DetectPointedAtGameObject )
        {
            goPointedAt = RaycastObject( Input.mousePosition );
        }

        if( Input.touchCount > 0 )
        {
            Touch touch = Input.GetTouch( 0 );
            this.currentPos = touch.position;

            if( touch.phase == TouchPhase.Began )
            {
                Press( touch.position );
            }
            else if( touch.phase == TouchPhase.Ended )
            {
                Release( touch.position );
            }

            return;
        }

        currentPos = Input.mousePosition;
        if( Input.GetMouseButtonDown( 0 ) )
        {
            Press( Input.mousePosition );
        }
        if( Input.GetMouseButtonUp( 0 ) )
        {
            Release( Input.mousePosition );
        }

        if( Input.GetMouseButtonDown( 1 ) )
        {
            pressedPosition = Input.mousePosition;
            lastGo = RaycastObject( pressedPosition );
            if( lastGo != null )
            {
                lastGo.SendMessage( "OnPressRight", SendMessageOptions.DontRequireReceiver );
            }
        }
    }


    private void Press( Vector2 screenPos )
    {
        pressedPosition = screenPos;
        this.Dragging = true;

        lastGo = RaycastObject( screenPos );
        if( lastGo != null )
        {
            lastGo.SendMessage( "OnPress", SendMessageOptions.DontRequireReceiver );
        }
    }

    private void Release( Vector2 screenPos )
    {
        if( lastGo != null )
        {
            GameObject currentGo = RaycastObject( screenPos );
            if( currentGo == lastGo ) lastGo.SendMessage( "OnClick", SendMessageOptions.DontRequireReceiver );
            lastGo.SendMessage( "OnRelease", SendMessageOptions.DontRequireReceiver );
            lastGo = null;
        }

        pressedPosition = Vector2.zero;
        this.Dragging = false;
    }

    private GameObject RaycastObject( Vector2 screenPos )
    {
        RaycastHit info;
        if( Physics.Raycast( m_Camera.ScreenPointToRay( screenPos ), out info, 200 ) )
        {
            inputHitPos = info.point;
            return info.collider.gameObject;
        }

        return null;
    }
}
