using System;
using Ju.Input;
using Ju.Log;
using UnityEngine;

public class PJ : MonoBehaviour
{
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;

    private Quaternion defaultPlayerRotation;
    private bool pjIn3D = false;

    private void Awake()
    {
        defaultPlayerRotation = transform.rotation;
    }

    public void DoMovement()
    {
        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.LeftArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.A))
        {
            transform.position -= transform.right * (Time.deltaTime * playerSpeed);
        } else if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.RightArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.D))
        {
            transform.position += transform.right * (Time.deltaTime * playerSpeed);
        }
        
        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.UpArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.W))
        {
            if (pjIn3D)
            {
                transform.position += transform.forward * (Time.deltaTime * playerSpeed);
            }
            else
            {
                transform.position += transform.up * (Time.deltaTime * playerSpeed);
            }

        } else if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.DownArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.S))
        {
            if (pjIn3D)
            {
                transform.position -= transform.forward * (Time.deltaTime * playerSpeed);
            }
            else
            {
                transform.position -= transform.up * (Time.deltaTime * playerSpeed);
            }
        }
    }

    public void DoRotation()
    {
        float mouseX;
        float mouseY;
        Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);
        transform.eulerAngles += playerRotationSpeed * new Vector3( /*-mouseY*/0, mouseX,0) ;
    }

    public void Switch2D3D()
    {
        if (transform.rotation == defaultPlayerRotation)
        {
            transform.Rotate(new Vector3(-90, 0, 0));
            pjIn3D = true;
        }
        else
        {
            transform.rotation = defaultPlayerRotation;
            pjIn3D = false;
        }
    }
}
