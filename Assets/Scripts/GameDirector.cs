using System.Linq;
using Ju.Input;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Serialization;

public class GameDirector : MonoBehaviour
{
    public PJ pj;
    public EnemyAI[] enemies;
    public CameraDirector cameraDirector;

    private bool gameIn3D = false;
    private Vector3 lastDirection = new Vector3();

    void Update()
    {
        if (!cameraDirector.CamerasTransitionBlending())
        {
            if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.C))
            {
                // Switch camera mode
                //TODO Hacer todos los cambios con eventos, igual que con los elementos de Environment
                gameIn3D = !gameIn3D;
                
                Core.Event.Fire(new SwitchPerspectiveEvent() {gameIn3D = gameIn3D});

                lastDirection = new Vector3();
                
                pj.Switch2D3D(gameIn3D);
                foreach  (EnemyAI enemy in enemies)
                {
                    enemy.Switch2D3D(gameIn3D);
                }
                cameraDirector.Switch2D3D(gameIn3D);
            } else
            {
                // Player
                Vector3 direction = GetMovementDirection();
                lastDirection = direction;
                
                pj.DoUpdate(direction);
                
                if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.E) ||
                    (Core.Input.Gamepads.ToArray().Length > 0 && Core.Input.Gamepads.First().IsButtonPressed(GamepadButton.B)))
                {
                    pj.Attack();
                }
                
                if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.RightShift) ||
                    (Core.Input.Gamepads.ToArray().Length > 0 && Core.Input.Gamepads.First().IsButtonPressed(GamepadButton.A)))
                {
                    pj.Roll(direction);
                }
                
                // Enemies
                if (gameIn3D)
                {
                    foreach  (EnemyAI enemy in enemies)
                    {
                        enemy.LookAtCamera();
                    }
                }
            }
        }
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = new Vector3();

        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.RightArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.D))
        {
            direction = pj.transform.right;
        } else if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.LeftArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.A))
        {
            direction = -pj.transform.right;
        }
            
        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.UpArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.W))
        {
            direction += pj.transform.forward;
        } else if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.DownArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.S))
        {
            direction += -pj.transform.forward;
        }
        
        return direction;
    }
}

public static class Layers
{
    public const int ENEMY_LAYER = 9;
    public const int LIGHT_LAYER = 10;
    public const int WEAPON_LAYER = 8;
    public const int WALL_LAYER = 7;
}

public struct SwitchPerspectiveEvent
{
    [FormerlySerializedAs("is3D")] public bool gameIn3D;
}