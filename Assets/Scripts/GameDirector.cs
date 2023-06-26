using Ju.Input;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    public PJ pj;
    public CameraDirector cameraDirector;

    private bool gameIn3D = false;

    void Update()
    {
        pj.DoMovement(gameIn3D);

        if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.Space))
        {
            gameIn3D = !gameIn3D;
            
            cameraDirector.Switch2D3D(gameIn3D);
            pj.Switch2D3D(gameIn3D);
        }

        if (gameIn3D)
        {
            pj.DoRotation();
        }
    }
}
