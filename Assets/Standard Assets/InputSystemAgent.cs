using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;


public class Reference<T> : IReadOnlyReference<T>
    where T : struct
{
    public T Value { get; set; }
}
public interface IReadOnlyReference<out T>
    where T : struct
{
    public T Value { get; }
}
public class InputSystemAgent : MonoBehaviour
{
    static InputSystemAgent _instance;
    public static InputSystemAgent Instance => _instance ??= FindObjectOfType<InputSystemAgent>();
    [SerializeField]
    private InputActionAsset defaultInputAsset;
    private void Awake()
    {
        _instance = this;
        EnhancedTouchSupport.Enable();
        var actions = defaultInputAsset.actionMaps[0].actions;
        defaultInputAsset?.Enable();
        ConfigureActions(actions);
    }
    private void ConfigureActions(ReadOnlyArray<InputAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.name.StartsWith("Key"))
            {
                var keyName = action.name["Key".Length..];
                //_isKeyDown[keyName] = _onKeyDown[keyName] = false;
                _key[keyName] = null;
                RegisterHandler(action, CreateOnKeyEvent(keyName));
            }
            else
                switch (action.name)
                {
                    case nameof(ViewMove):
                        RegisterHandler(action, CreateValueEvent(_viewMove));
                        break;
                    case nameof(FlyMove):
                        RegisterHandler(action, CreateValueEvent(_flyMove));
                        break;
                    case nameof(NormalMove):
                        RegisterHandler(action, CreateValueEvent(_normalMove));
                        break;
                    case nameof(DesiredDistance):
                        RegisterHandler(action, CreateValueEvent(_desiredDistance));
                        break;
                    default:
                        Debug.LogWarning($"Unhandled Action {action.name}");
                        break;
                }
        }
    }

    public static float DesiredDistance => Instance._desiredDistance.Value;
    public static Vector2 ViewMove => Instance._viewMove.Value;
    public static Vector2 NormalMove => Instance._normalMove.Value;
    public static Vector3 FlyMove => Instance._flyMove.Value;
    //public static IReadOnlyDictionary<string, bool> IsKeyDown => Instance._isKeyDown;
    //private readonly Dictionary<string, bool> _isKeyDown = new();
    //public static IReadOnlyDictionary<string, bool> OnKeyDown => Instance._onKeyDown;
    //private readonly Dictionary<string, bool> _onKeyDown = new();
    public static IReadOnlyDictionary<string, ButtonControl?> Key => Instance._key;
    private readonly Dictionary<string, ButtonControl?> _key = new();
    public static bool GetKey(string key) => Key[key]?.isPressed ?? false;
    public static bool GetKeyDown(string key) => Key[key]?.wasPressedThisFrame ?? false;
    public static bool GetKeyUp(string key) => Key[key]?.wasReleasedThisFrame ?? false;
    private readonly Reference<float> _desiredDistance = new();
    private readonly Reference<Vector2> _viewMove = new();
    private readonly Reference<Vector2> _normalMove = new();
    private readonly Reference<Vector3> _flyMove = new();

    private void RegisterHandler(InputAction action, Action<InputAction.CallbackContext> handler)
    {
        action.performed += handler;
        action.started += handler;
        action.canceled += handler;
    }
    private static Action<InputAction.CallbackContext> CreateValueEvent<T>(Reference<T> writeTo)
        where T : struct
    {
        void _handler(InputAction.CallbackContext context)
        {
            writeTo.Value = context.ReadValue<T>();
        }
        return _handler;
    }
    private Action<InputAction.CallbackContext> CreateOnKeyEvent(string key)
    {
        void _handler(InputAction.CallbackContext context)
        {
            _key[key] = context.control as ButtonControl;
        }
        return _handler;
    }
    private void OnEnable()
    {
        //defaultInputAsset.Enable();
    }
    private void OnDisable()
    {
        //defaultInputAsset.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
