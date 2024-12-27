using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : Player.PlayerComponent
{
    [SerializeField] private Vector2 mouseSensitivity;
    [SerializeField] private bool flipX, flipY;

    public Vector2 Inputs {
        get {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }

    public bool Inputting => Inputs != Vector2.zero;

    public Vector2 MouseDelta {
        get {
            return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        }
    }

    public Vector2 AlteredMouseDelta
    {
        get {
            return new Vector2(MouseDelta.x * mouseSensitivity.y * (flipX ? -1 : 1), MouseDelta.y * mouseSensitivity.x * (flipY ? -1 : 1));
        }
    }

    public bool MouseLock
    {
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    public (bool Pressed, bool Held) Jump {
        get {
            return (Input.GetKeyDown(KeyCode.Space), Input.GetKey(KeyCode.Space));
        }
    }

    public bool Roll {
        get {
            return Input.GetKey(KeyCode.R);
        }
    }
}
