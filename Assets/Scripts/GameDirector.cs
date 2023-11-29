using System.Collections;
using System.Collections.Generic;
using Ju.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using InputAction = UnityEngine.InputSystem.InputAction;

public class GameDirector : MonoBehaviour
{
    #region Public variables
    
    public bool debugMode;
    public float timeLoopDuration = 10f;
    public GameObject moon;
    public Canvas canvas;
    public PostProcessVolume postprocessing;
    public NarrativeDirector narrativeDirector;
    public GameObject directionalLights;
    public ControlScheme control = null;
    
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

    [System.Serializable]
    public class DialogueDictionary : SerializableDictionary<DialogueSO, int> { }
    
    [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Diálogo", ValueLabel = "Valor")]
    public DialogueDictionary lateralDialogs = new DialogueDictionary();
    
    //[ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Diálogo", ValueLabel = "Segundo de aparición")]
    //public Dictionary<DialogueSO, float> lateralDialogs = new Dictionary<DialogueSO, float>();
    
    #endregion
    
    #region Private variables

    private PJ pj;
    private CameraDirector cameraDirector;
    private bool gameIn3D;
    private bool isInitialLoad = true;
    private bool isFirstFloorLoad = true;
    private bool timeLoopEnded;
    private float initialTimeLoopDuration;
    private float secondsCounter = 0;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    
    private Vector2 inputDirection = Vector2.zero;
    
    public PlayerInput playerInput;
    
    //INPUT ACTIONS
    private InputAction MoveAction;
    private InputAction RollAction;
    private InputAction InteractAction;
    private InputAction CameraChangeAction;
    private InputAction CameraRotationAction;

    #endregion

    #region Unity Events

    private void Awake()
    {
        initialTimeLoopDuration = timeLoopDuration;

        if (!debugMode)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        var moonRotation = moon.transform.rotation;

        Core.Dialogue.Initialize(canvas);

        this.EventSubscribe<GameEvents.EnemyDied>(e => CheckEnemiesInScene());
        this.EventSubscribe<GameEvents.NPCVanished>(e => ShowGodNarrative());
        this.EventSubscribe<GameEvents.DoorOpened>(e => DoorOpened());
        this.EventSubscribe<GameEvents.PlayerDamaged>(e => PlayerDamaged());

        UpdateGameState();

        Core.Audio.Play(SOUND_TYPE.BackgroundMusic, 1f, 0.2f);
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

        if (SceneManager.GetActiveScene().name == "T1C0F0")
        {
            StartT1C0F0GameFlow();
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
            StartCoroutine(WaitToInitializeGameDirectorAndSet3D());
            isFirstFloorLoad = false;
        }
        else
        {
            StartCoroutine(WaitToInitializeGameDirector());
        }
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
            if (timeLoopDuration > 0 && !timeLoopEnded)
            {
                timeLoopDuration -= Time.deltaTime;
                UpdateLighthouseRotation();

                secondsCounter += Time.deltaTime;
                if (secondsCounter >= 1)
                {
                    secondsCounter = 0;
                    Core.Audio.Play(SOUND_TYPE.ClockTikTak, 2, 0.05f);
                }
            }
            else if (!timeLoopEnded)
            {
                RestartTimeLoop();
            }
        }
    }

    #endregion

    #region Gameflow methods

    private void StartT1C0F0GameFlow()
    {
        Core.PositionRecorder.StartRecording(pj.transform, moon.transform);
        Core.Dialogue.ShowLateralDialogs(lateralDialogs);
    }
    
    private void UpdateGameState()
    {
        GameState.gameIn3D = this.gameIn3D;
    }
    
    private void UpdateLighthouseRotation()
    {
        float pendingTimeLoopDurationPorcentage = timeLoopDuration / initialTimeLoopDuration;
        float nextLighthoyseYRotation = 360f * pendingTimeLoopDurationPorcentage * -1;

        // Make sure the rotation value stays within 0 to 360 degrees
        //currentLighthouseYRotation = currentLighthouseYRotation % 360.0f;

        // Apply the rotation to the GameObject
        moon.transform.eulerAngles = new Vector3(moon.transform.eulerAngles.x, nextLighthoyseYRotation, moon.transform.eulerAngles.z);
    }

    private void RestartTimeLoop()
    {
        timeLoopEnded = true;
        timeLoopDuration = initialTimeLoopDuration;
        
        if (SceneManager.GetActiveScene().name != "T1C0F0" && !debugMode)
        {
            isFirstFloorLoad = true;
            Core.Event.Fire<GameEvents.LoadInitialFloorSceneEvent>();
        }
        else if (SceneManager.GetActiveScene().name == "T1C0F0")
        {
            Core.PositionRecorder.StopRecording();
            Core.PositionRecorder.DoRewind(pj.transform, moon.transform);
        }
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

    #endregion

    #region Event callbacks
    
    private void PlayerDamaged()
    {
        timeLoopDuration -= 10;
        
        // TODO Should gamefeel: modificar el postprocesado cuando el jugador es atacado
        //postprocessing.profile.TryGetSettings(out bloom);
        //postprocessing.profile.TryGetSettings(out chromaticAberration);
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
    
    private void CheckEnemiesInScene()
    {
        SwitchGamePerspective();
    }
    
    #endregion
    
    #region Utils
    
    private IEnumerator WaitToInitializeGameDirectorAndSet3D()
    {
        yield return null;

        InitializeGameDirector();
        bool gameIn3D = true;
        SetGameState(gameIn3D);
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
        SetGameState(!gameIn3D);
    }

    private void SetGameState(bool gameIn3D)
    {
        this.gameIn3D = gameIn3D;
        UpdateGameState();

        Core.Event.Fire(new GameEvents.SwitchPerspectiveEvent() { gameIn3D = this.gameIn3D });
    }

    #endregion
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private List<TKey> keys = new List<TKey>();

    [SerializeField, HideInInspector]
    private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0; i < keys.Count; i++)
        {
            this[keys[i]] = values[i];
        }
    }
}