using Ju.Input;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    public PJ pj;
    public EnemyAI[] enemies;
    public CameraDirector cameraDirector;
    public EnvironmentController environmentController;

    private bool gameIn3D = false;
    private Vector3 lastDirection = new Vector3();

    void Update()
    {
        if (!cameraDirector.CamerasTransitionBlending() && Core.Input.Keyboard.IsKeyPressed(KeyboardKey.Space))
        {
            gameIn3D = !gameIn3D;
            lastDirection = new Vector3();
            
            pj.Switch2D3D(gameIn3D);
            foreach  (EnemyAI enemy in enemies)
            {
                enemy.Switch2D3D(gameIn3D);
            }
            cameraDirector.Switch2D3D(gameIn3D);
            environmentController.Switch2D3D(gameIn3D);
        } else if (!cameraDirector.CamerasTransitionBlending())
        {
            Vector3 direction = GetMovementDirection();
            
            pj.DoMovement(direction);

            pj.DoRotation(gameIn3D, lastDirection);
            
            if (gameIn3D)
            {
                foreach  (EnemyAI enemy in enemies)
                {
                    enemy.LookAtCamera();
                }
            }

            if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.E))
            {
                pj.Attack();
            }
        }
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = new Vector3();

        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.RightArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.D))
        {
            direction = pj.transform.right;
            lastDirection = direction;
        } else if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.LeftArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.A))
        {
            direction = -pj.transform.right;
            lastDirection = direction;
        }
            
        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.UpArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.W))
        {
            direction += pj.transform.forward;
            lastDirection = direction;
        } else if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.DownArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.S))
        {
            direction += -pj.transform.forward;
            lastDirection = direction;
        }
        
        return direction;
    }
}

public static class Layers
{
    public const int ENEMY_LAYER = 9;
    public const int LIGHT_LAYER = 10;
    public const int WEAPON_LAYER = 8;
}