//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/Scripts/Runtime/Input/PlayerInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInput : IInputActionCollection2, IDisposable
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
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""24b77935-6aab-4da6-abf3-003aff285874"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""RewindTime"",
                    ""type"": ""Button"",
                    ""id"": ""c78b4cfd-9862-49cc-b6a0-c1f80684fb11"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""29b832d2-7d3a-4c89-b946-cbffbc6584e8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Roll"",
                    ""type"": ""Button"",
                    ""id"": ""ef07f34c-3af3-4ba3-984d-cbc9af23f83c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""WallRun"",
                    ""type"": ""Button"",
                    ""id"": ""0bc605db-c802-4393-a0f3-cbb4a4b0f00f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""28bf60dc-568b-40c9-9545-7749300199f7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Sheathe"",
                    ""type"": ""Button"",
                    ""id"": ""42514c0b-cabc-4b94-9af0-5bd2416fdbb6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Block"",
                    ""type"": ""Button"",
                    ""id"": ""223a010b-5c34-412c-bba8-0295b9347a61"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
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
                },
                {
                    ""name"": """",
                    ""id"": ""1b557da4-8661-4666-a807-1900b286e9d4"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""WallRun"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""994fa202-51d1-4089-818e-84b5c57d64d7"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""715de614-e5d4-47f3-9124-187a7bbc2a58"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sheathe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6cfdd61d-bea3-4682-aa43-55fc9507a62b"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e35f2479-0019-400f-8424-d2f1bdeb09c6"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Block"",
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
        m_ActionMap_Roll = m_ActionMap.FindAction("Roll", throwIfNotFound: true);
        m_ActionMap_WallRun = m_ActionMap.FindAction("WallRun", throwIfNotFound: true);
        m_ActionMap_Attack = m_ActionMap.FindAction("Attack", throwIfNotFound: true);
        m_ActionMap_Sheathe = m_ActionMap.FindAction("Sheathe", throwIfNotFound: true);
        m_ActionMap_Block = m_ActionMap.FindAction("Block", throwIfNotFound: true);
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
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // ActionMap
    private readonly InputActionMap m_ActionMap;
    private IActionMapActions m_ActionMapActionsCallbackInterface;
    private readonly InputAction m_ActionMap_CameraAim;
    private readonly InputAction m_ActionMap_Move;
    private readonly InputAction m_ActionMap_RewindTime;
    private readonly InputAction m_ActionMap_Jump;
    private readonly InputAction m_ActionMap_Roll;
    private readonly InputAction m_ActionMap_WallRun;
    private readonly InputAction m_ActionMap_Attack;
    private readonly InputAction m_ActionMap_Sheathe;
    private readonly InputAction m_ActionMap_Block;
    public struct ActionMapActions
    {
        private @PlayerInput m_Wrapper;
        public ActionMapActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraAim => m_Wrapper.m_ActionMap_CameraAim;
        public InputAction @Move => m_Wrapper.m_ActionMap_Move;
        public InputAction @RewindTime => m_Wrapper.m_ActionMap_RewindTime;
        public InputAction @Jump => m_Wrapper.m_ActionMap_Jump;
        public InputAction @Roll => m_Wrapper.m_ActionMap_Roll;
        public InputAction @WallRun => m_Wrapper.m_ActionMap_WallRun;
        public InputAction @Attack => m_Wrapper.m_ActionMap_Attack;
        public InputAction @Sheathe => m_Wrapper.m_ActionMap_Sheathe;
        public InputAction @Block => m_Wrapper.m_ActionMap_Block;
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
                @Roll.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRoll;
                @Roll.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRoll;
                @Roll.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRoll;
                @WallRun.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnWallRun;
                @WallRun.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnWallRun;
                @WallRun.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnWallRun;
                @Attack.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnAttack;
                @Sheathe.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnSheathe;
                @Sheathe.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnSheathe;
                @Sheathe.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnSheathe;
                @Block.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnBlock;
                @Block.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnBlock;
                @Block.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnBlock;
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
                @Roll.started += instance.OnRoll;
                @Roll.performed += instance.OnRoll;
                @Roll.canceled += instance.OnRoll;
                @WallRun.started += instance.OnWallRun;
                @WallRun.performed += instance.OnWallRun;
                @WallRun.canceled += instance.OnWallRun;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @Sheathe.started += instance.OnSheathe;
                @Sheathe.performed += instance.OnSheathe;
                @Sheathe.canceled += instance.OnSheathe;
                @Block.started += instance.OnBlock;
                @Block.performed += instance.OnBlock;
                @Block.canceled += instance.OnBlock;
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
        void OnRoll(InputAction.CallbackContext context);
        void OnWallRun(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnSheathe(InputAction.CallbackContext context);
        void OnBlock(InputAction.CallbackContext context);
    }
}
