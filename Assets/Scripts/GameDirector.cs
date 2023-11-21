using System;
using System.Collections;
using System.Linq;
using Ju.Input;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
using InputAction = UnityEngine.InputSystem.InputAction;

public class GameDirector : MonoBehaviour
{
    public bool debugMode;
    public float timeLoopDuration = 10f;
    public GameObject lightHouse;
    public Canvas canvas;
    public PostProcessVolume postprocessing;
    public NarrativeDirector narrativeDirector;
    public GameObject directionalLights;

    private PJ pj;
    private CameraDirector cameraDirector;
    private bool gameIn3D = false;
    private Vector3 lastDirection = new Vector3();
    private bool isInitialLoad = true;
    private bool isFirstFloorLoad = true;
    private float initialTimeLoopDuration;
    private float currentLighthouseYRotation;
    private float initialLighthouseXRotation;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;

    public ControlScheme control = null;
    private Vector2 inputDirection = Vector2.zero;
    
    public PlayerInput playerInput;
    
    //INPUT ACTIONS
    private InputAction MoveAction;
    private InputAction RollAction;
    private InputAction InteractAction;
    private InputAction CameraChangeAction;
    private InputAction CameraRotationAction;
    
    public struct ControlInputData
    {
        public Vector3 movementDirection;
        public Vector3 inputDirection;
        public Vector2 cameraRotation;

        public ControlInputData(Vector3 movementDirection, Vector3 inputDirection, Vector2 cameraRotation)
        {
            this.movementDirection = movementDirection;
            this.inputDirection = inputDirection;
            this.cameraRotation = cameraRotation;
        }
    }
    
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

        Core.Dialogue.Initialize(canvas);
        
        this.EventSubscribe<GameEvents.EnemyDied>(e => CheckEnemiesInScene());
        this.EventSubscribe<GameEvents.NPCVanished>(e => ShowGodNarrative());
        this.EventSubscribe<GameEvents.DoorOpened>(e => DoorOpened());
        this.EventSubscribe<GameEvents.PlayerDamaged>(e => PlayerDamaged());

        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(lightHouse.gameObject);

        UpdateGameState();
    }
    
    private void PlayerDamaged()
    {
        timeLoopDuration -= 10;
        
        // TODO Should gamefeel: modificar el postprocesado cuando el jugador es atacado
        postprocessing.profile.TryGetSettings(out bloom);
        postprocessing.profile.TryGetSettings(out chromaticAberration);
    }

    private void DoorOpened()
    {
        directionalLights.SetActive(false);
        SwitchGamePerspective();
    }

    private void ShowGodNarrative()
    {
        narrativeDirector.ShowNarrative();
    }

    private void UpdateGameState()
    {
        GameState.gameIn3D = this.gameIn3D;
    }

    private void CheckEnemiesInScene()
    {
        /*EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        if (enemies.Length <= 1)
        {
            SwitchGamePerspective();
        }*/
        
        SwitchGamePerspective();
    }

    private void Start()
    {
        if (!debugMode)
        {
            Core.Event.Fire(new GameEvents.LoadInitialFloorSceneEvent());
        }
        else
        {
            InitializeGameDirector();
        }

        if (playerInput)
        {
            MoveAction = playerInput.actions["Move"];
            RollAction = playerInput.actions["Roll"];
            InteractAction = playerInput.actions["Action"];
            CameraChangeAction = playerInput.actions["CameraChange"];
            CameraRotationAction = playerInput.actions["CameraRotation"];
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
        UpdateGameState();
                
        Core.Event.Fire(new GameEvents.SwitchPerspectiveEvent() {gameIn3D = gameIn3D});

        lastDirection = new Vector3();
    }

    void Update()
    {
        if (cameraDirector != null && !cameraDirector.CamerasTransitionBlending())
        {
            if (debugMode && CameraChangeAction.triggered)
            {
                SwitchGamePerspective();
            } else if (pj != null && !narrativeDirector.IsShowingNarrative())
            {
                // Player
                ControlInputData controlInputData = GetControlInputDataValues();
                
                pj.DoUpdate(controlInputData);
                
                if (InteractAction.triggered)
                {
                    pj.DoMainAction();
                }
                
                if (RollAction.triggered)
                {
                    pj.DoRoll(controlInputData.movementDirection);
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
        float pendingTimeLoopDurationPorcentage = timeLoopDuration / initialTimeLoopDuration;
        float nextLighthoyseYRotation = 360f * pendingTimeLoopDurationPorcentage * -1;

        // Make sure the rotation value stays within 0 to 360 degrees
        //currentLighthouseYRotation = currentLighthouseYRotation % 360.0f;

        // Apply the rotation to the GameObject
        lightHouse.transform.eulerAngles = new Vector3(lightHouse.transform.eulerAngles.x, nextLighthoyseYRotation, lightHouse.transform.eulerAngles.z);
    }

    private void RestartTimeLoop()
    {
        isFirstFloorLoad = true;
        Core.Event.Fire<GameEvents.LoadInitialFloorSceneEvent>();
    }

    private ControlInputData GetControlInputDataValues()
    {
        inputDirection = MoveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3();
        
        if (inputDirection.x > 0)
        {
            direction += pj.transform.right;
        } else if (inputDirection.x < 0)
        {
            direction += -pj.transform.right;
        }
            
        if (inputDirection.y > 0)
        {
            direction += pj.transform.forward;
        } else if (inputDirection.y < 0)
        {
            direction += -pj.transform.forward;
        }

        return new ControlInputData(direction, inputDirection, CameraRotationAction.ReadValue<Vector2>());
    }
}