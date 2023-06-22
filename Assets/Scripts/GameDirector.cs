using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Ju.Input;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    public PJ pj;
    public Camera2D camera2D;
    public Camera3D camera3D;

    // Update is called once per frame
    void Update()
    {
        pj.DoMovement();
        camera2D.ChaseTarget();

        if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.Space))
        {
            camera2D.SwitchCamera();
            camera3D.SwitchCamera();
            pj.Switch2D3D();
        }

        if (!camera2D.IsEnabled())
        {
            pj.DoRotation();
        }
    }
}
