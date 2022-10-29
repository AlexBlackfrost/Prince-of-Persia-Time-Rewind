using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {
    private PlayerInput playerInput;

    private void Awake() {
        playerInput = new PlayerInput();
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
}

