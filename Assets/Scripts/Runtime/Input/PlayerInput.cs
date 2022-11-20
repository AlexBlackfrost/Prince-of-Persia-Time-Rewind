// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Runtime/Input/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""ActionMap"",
            ""id"": ""86339911-bb4d-4bfe-a5ca-07a73d3452f8"",
            ""actions"": [
                {
                    ""name"": ""CameraAim"",
                    ""type"": ""PassThrough"",
                    ""id"": ""b2f52869-3071-4756-9d05-d4409663ee9f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""24b77935-6aab-4da6-abf3-003aff285874"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RewindTime"",
                    ""type"": ""Button"",
                    ""id"": ""c78b4cfd-9862-49cc-b6a0-c1f80684fb11"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""29b832d2-7d3a-4c89-b946-cbffbc6584e8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ea9409e8-8412-4132-971c-c5433b485919"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraAim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""0bfd1c47-31c8-4998-b2d7-ecb125ec5795"",
                    ""path"": ""2DVectorCompositeNoDirectionalCancelling(mode=1)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Up"",
                    ""id"": ""39694930-ecf3-4da6-8de4-6d95b76e8191"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Down"",
                    ""id"": ""b8d8fe6a-33ba-47b2-9424-d7fda6315a12"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Left"",
                    ""id"": ""7c25664f-881a-4e12-b62c-79b014a18e1d"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Right"",
                    ""id"": ""e9244cef-9bf6-47b1-b819-5102971d1ff4"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""fa89cfb5-84ab-4567-9fa7-a6fcbd982c18"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RewindTime"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c336c95-39de-42c0-a1c1-f527358e0b28"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // ActionMap
        m_ActionMap = asset.FindActionMap("ActionMap", throwIfNotFound: true);
        m_ActionMap_CameraAim = m_ActionMap.FindAction("CameraAim", throwIfNotFound: true);
        m_ActionMap_Move = m_ActionMap.FindAction("Move", throwIfNotFound: true);
        m_ActionMap_RewindTime = m_ActionMap.FindAction("RewindTime", throwIfNotFound: true);
        m_ActionMap_Jump = m_ActionMap.FindAction("Jump", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // ActionMap
    private readonly InputActionMap m_ActionMap;
    private IActionMapActions m_ActionMapActionsCallbackInterface;
    private readonly InputAction m_ActionMap_CameraAim;
    private readonly InputAction m_ActionMap_Move;
    private readonly InputAction m_ActionMap_RewindTime;
    private readonly InputAction m_ActionMap_Jump;
    public struct ActionMapActions
    {
        private @PlayerInput m_Wrapper;
        public ActionMapActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraAim => m_Wrapper.m_ActionMap_CameraAim;
        public InputAction @Move => m_Wrapper.m_ActionMap_Move;
        public InputAction @RewindTime => m_Wrapper.m_ActionMap_RewindTime;
        public InputAction @Jump => m_Wrapper.m_ActionMap_Jump;
        public InputActionMap Get() { return m_Wrapper.m_ActionMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ActionMapActions set) { return set.Get(); }
        public void SetCallbacks(IActionMapActions instance)
        {
            if (m_Wrapper.m_ActionMapActionsCallbackInterface != null)
            {
                @CameraAim.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnCameraAim;
                @CameraAim.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnCameraAim;
                @CameraAim.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnCameraAim;
                @Move.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnMove;
                @RewindTime.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRewindTime;
                @RewindTime.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRewindTime;
                @RewindTime.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRewindTime;
                @Jump.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_ActionMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @CameraAim.started += instance.OnCameraAim;
                @CameraAim.performed += instance.OnCameraAim;
                @CameraAim.canceled += instance.OnCameraAim;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @RewindTime.started += instance.OnRewindTime;
                @RewindTime.performed += instance.OnRewindTime;
                @RewindTime.canceled += instance.OnRewindTime;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public ActionMapActions @ActionMap => new ActionMapActions(this);
    public interface IActionMapActions
    {
        void OnCameraAim(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnRewindTime(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
