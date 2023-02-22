using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InputController : MonoBehaviour {
    private PlayerInput playerInput;
    public InputAction TimeRewind{ get; private set;}
    public InputAction Jump { get; private set;  }
    public InputAction WallRun { get; private set;  }
    public InputAction Attack { get; private set;  }

    private void Awake() {
        playerInput = new PlayerInput();
        TimeRewind = playerInput.ActionMap.RewindTime;
        Jump = playerInput.ActionMap.Jump;
        WallRun = playerInput.ActionMap.WallRun;
        Attack = playerInput.ActionMap.Attack;
    }
    

    private void OnEnable() {
        playerInput.ActionMap.Enable();
    }

    private void OnDisable() {
        playerInput.ActionMap.Disable();
    }

    public bool IsMoving() {
        return playerInput.ActionMap.Move.ReadValue<Vector2>().magnitude > 0;
    }

    public Vector2 GetMoveDirection() {
        return playerInput.ActionMap.Move.ReadValue<Vector2>();
    }

    public bool IsTimeRewindPressed() {
        return TimeRewind.ReadValue<float>() > 0;
    }

    public bool IsWallRunPressed() {
        return WallRun.ReadValue<float>() > 0;
    }
    public bool IsWallRunNotPressed() {
        return !IsWallRunPressed();
    }

    public bool IsAttackPressed() {
        return Attack.ReadValue<float>() > 0;
    }
}

