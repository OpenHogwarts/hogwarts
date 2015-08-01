using UnityEngine;
using UnityEngine.UI;

/**
 Based on SunCube Helicopter controller
 https://www.assetstore.unity3d.com/en/#!/content/40107
**/

public class BroomstickController : MonoBehaviour
{
	public AudioSource HelicopterSound;
	public Rigidbody HelicopterModel;
	
	public float TurnForce = 3f;
	public float ForwardForce = 10f;
	public float ForwardTiltForce = 20f;
	public float TurnTiltForce = 30f;
	public float EffectiveHeight = 100f;
	
	public float turnTiltForcePercent = 1.5f;
	public float turnForcePercent = 1.3f;
	
	private float _engineForce;
	public float EngineForce
	{
		get { return _engineForce; }
		set
		{
			HelicopterSound.pitch = Mathf.Clamp(value / 40, 0, 1.2f);
			_engineForce = value;
		}
	}
	
	private Vector2 hMove = Vector2.zero;
	private Vector2 hTilt = Vector2.zero;
	private float hTurn = 0f;
	public bool IsOnGround = true;
	
	private const KeyCode SpeedUp = KeyCode.LeftShift;
	private const KeyCode SpeedDown = KeyCode.Space;
	private const KeyCode Forward = KeyCode.W;
	private const KeyCode Back = KeyCode.S;
	private const KeyCode Left = KeyCode.A;
	private const KeyCode Right = KeyCode.D;
	private const KeyCode TurnLeft = KeyCode.Q;
	private const KeyCode TurnRight = KeyCode.E;
	private KeyCode[] keyCodes;

	private void Awake()
	{
		keyCodes = new[] {
			SpeedUp,
			SpeedDown,
			Forward,
			Back,
			Left,
			Right,
			TurnLeft,
			TurnRight
		};
	}

	
	void FixedUpdate()
	{
		LiftProcess();
		MoveProcess();
		TiltProcess();
	}
	
	private void MoveProcess()
	{
		var turn = TurnForce * Mathf.Lerp(hMove.x, hMove.x * (turnTiltForcePercent - Mathf.Abs(hMove.y)), Mathf.Max(0f, hMove.y));
		hTurn = Mathf.Lerp(hTurn, turn, Time.fixedDeltaTime * TurnForce);

		HelicopterModel.AddRelativeTorque(0f, hTurn * HelicopterModel.mass, 0f);
		HelicopterModel.AddRelativeForce(Vector3.forward * Mathf.Max(0f, hMove.y * ForwardForce * HelicopterModel.mass));
	}
	
	private void LiftProcess()
	{
		var upForce = 1 - Mathf.Clamp(HelicopterModel.transform.position.y / EffectiveHeight, 0, 1);
		upForce = Mathf.Lerp(0f, EngineForce, upForce) * (HelicopterModel.mass);
		HelicopterModel.AddRelativeForce(Vector3.up * upForce);
	}
	
	private void TiltProcess()
	{
		hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * TurnTiltForce, Time.deltaTime);
		hTilt.y = Mathf.Lerp(hTilt.y, hMove.y * ForwardTiltForce, Time.deltaTime);
		HelicopterModel.transform.localRotation = Quaternion.Euler(hTilt.y, HelicopterModel.transform.localEulerAngles.y, -hTilt.x);
	}
	
	private void Update()
	{
		float tempY = 0;
		float tempX = 0;
		
		// stable forward
		if (hMove.y > 0) {
			tempY = - Time.fixedDeltaTime;
		} else if (hMove.y < 0) {
			tempY = Time.fixedDeltaTime;
		}
		
		// stable lurn
		if (hMove.x > 0) {
			tempX = -Time.fixedDeltaTime;
		} else if (hMove.x < 0) {
			tempX = Time.fixedDeltaTime;
		}
		
		
		foreach (var keyCode in keyCodes)
		{
			if (!Input.GetKey(keyCode)) {
				continue;
			}

			switch (keyCode)
			{
			case SpeedUp:
				EngineForce += 0.1f;
				break;
			case SpeedDown:
				
				EngineForce -= 0.12f;
				if (EngineForce < 0) EngineForce = 0;
				break;
				
			case Forward:
				
				if (IsOnGround) break;
				tempY = Time.fixedDeltaTime;
				break;
			case Back:
				
				if (IsOnGround) break;
				tempY = -Time.fixedDeltaTime;
				break;
			case Left:
				
				if (IsOnGround) break;
				tempX = -Time.fixedDeltaTime;
				break;
			case Right:
				
				if (IsOnGround) break;
				tempX = Time.fixedDeltaTime;
				break;
			case TurnRight:
			{
				if (IsOnGround) break;
				var force = (turnForcePercent - Mathf.Abs(hMove.y))*HelicopterModel.mass;
				HelicopterModel.AddRelativeTorque(0f, force, 0);
			}
				break;
			case TurnLeft:
			{
				if (IsOnGround) break;
				
				var force = -(turnForcePercent - Mathf.Abs(hMove.y))*HelicopterModel.mass;
				HelicopterModel.AddRelativeTorque(0f, force, 0);
			}
				break;
				
			}
		}
		
		hMove.x += tempX;
		hMove.x = Mathf.Clamp(hMove.x, -1, 1);
		
		hMove.y += tempY;
		hMove.y = Mathf.Clamp(hMove.y, -1, 1);
		
	}
	
	private void OnCollisionEnter()
	{
		IsOnGround = true;
	}
	
	private void OnCollisionExit()
	{
		IsOnGround = false;
	}
}