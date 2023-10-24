using System.Collections;
using System.Linq;
using Ju.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    public bool debugMode;
    public float timeLoopDuration = 10f;
    public GameObject lightHouse;
    public EnemyAI[] enemies;

    private PJ pj;
    private CameraDirector cameraDirector;
    private bool gameIn3D = false;
    private Vector3 lastDirection = new Vector3();
    private GamepadController gamepad;
    private bool isInitialLoad = true;
    private bool isFirstFloorLoad = true;
    private float initialTimeLoopDuration;
    private float currentLighthouseYRotation;
    private float initialLighthouseXRotation;

    private void Awake()
    {
        initialTimeLoopDuration = timeLoopDuration;
        if (!debugMode)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        var lighthouseRotation = lightHouse.transform.rotation;
        currentLighthouseYRotation = lighthouseRotation.eulerAngles.y;
        initialLighthouseXRotation = lighthouseRotation.eulerAngles.x;
        
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(lightHouse.gameObject);
    }
    
    private void Start()
    {
        if (Core.Input.Gamepads.Any())
        {
            gamepad = (GamepadController)Core.Input.Gamepads.First();
        }
        else
        {
            gamepad = null;
        }

        if (!debugMode)
        {
            Core.Event.Fire(new GameEvents.LoadInitialFloorSceneEvent());
        }
        else
        {
            InitializeGameDirector();
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (isInitialLoad)
        {
            isInitialLoad = false;
        } else if (!isInitialLoad && isFirstFloorLoad)
        {
            StartCoroutine(WaitToInitializeGameDirectorAndSwitchPerspective());
            isFirstFloorLoad = false;
        }
        else
        {
            StartCoroutine(WaitToInitializeGameDirector());
        }
    }

    private IEnumerator WaitToInitializeGameDirectorAndSwitchPerspective()
    {
        yield return null;

        InitializeGameDirector();
        SwitchGamePerspective();
    }

    private IEnumerator WaitToInitializeGameDirector()
    {
        yield return null;

        InitializeGameDirector();
    }

    private void InitializeGameDirector()
    {
        if (Camera.main != null)
        {
            cameraDirector = Camera.main.GetComponent<CameraDirector>();
        }
        else
        {
            cameraDirector = null;
        }

        var player = FindObjectOfType<PJ>();
        if (player != null)
        {
            pj = player;
        }
        else
        {
            pj = null;
        }

        cameraDirector.Initialize(pj.transform);
    }

    private void SwitchGamePerspective()
    {
        gameIn3D = !gameIn3D;
                
        Core.Event.Fire(new GameEvents.SwitchPerspectiveEvent() {gameIn3D = gameIn3D});

        lastDirection = new Vector3();
                
        pj.Switch2D3D(gameIn3D);
        foreach  (EnemyAI enemy in enemies)
        {
            enemy.Switch2D3D(gameIn3D);
        }
        cameraDirector.Switch2D3D(gameIn3D);
    }

    void Update()
    {
        if (cameraDirector != null && !cameraDirector.CamerasTransitionBlending())
        {
            if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.C))
            {
                SwitchGamePerspective();
            } else if (pj != null)
            {
                // Player
                Vector3 direction = GetMovementDirection();
                lastDirection = direction;
                
                pj.DoUpdate(direction);
                
                if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.E) ||
                    (Core.Input.Gamepads.ToArray().Length > 0 && gamepad != null && gamepad.IsButtonPressed(GamepadButton.B)))
                {
                    pj.DoMainAction();
                }
                
                if (Core.Input.Keyboard.IsKeyPressed(KeyboardKey.RightShift) ||
                    (Core.Input.Gamepads.ToArray().Length > 0 && gamepad != null && gamepad.IsButtonPressed(GamepadButton.A)))
                {
                    pj.DoRoll(direction);
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
        
        // If time loop is not disabled
        if (timeLoopDuration != -1)
        {
            if (timeLoopDuration > 0)
            {
                timeLoopDuration -= Time.deltaTime;
                UpdateLighthouseRotation();
            }
            else
            {
                timeLoopDuration = initialTimeLoopDuration;
                RestartTimeLoop();
            }
        }
    }

    private void UpdateLighthouseRotation()
    {
        float rotationSpeed = 360.0f / initialTimeLoopDuration;
        
        currentLighthouseYRotation += rotationSpeed * Time.deltaTime;

        // Make sure the rotation value stays within 0 to 360 degrees
        currentLighthouseYRotation = currentLighthouseYRotation % 360.0f;

        // Apply the rotation to the GameObject
        lightHouse.transform.rotation = Quaternion.Euler(initialLighthouseXRotation, currentLighthouseYRotation, 0);
    }

    private void RestartTimeLoop()
    {
        isFirstFloorLoad = true;
        Core.Event.Fire<GameEvents.LoadInitialFloorSceneEvent>();
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