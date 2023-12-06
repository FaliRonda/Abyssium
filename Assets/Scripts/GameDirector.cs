using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject audioGO;
    public ControlScheme control = null;
    
    public float cycle1LoopDuration = 120f;
    public float cycle2LoopDuration = 300f;
    
    public PlayerInput playerInput;
    
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
    
    [System.Serializable]
    public class SceneDialogueDictionary : SerializableDictionary<string, DialogueDictionary> { }
    
    [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Escena", ValueLabel = "Lateral Dialogs")]
    public SceneDialogueDictionary sceneLateralDialogs = new SceneDialogueDictionary();
    

    #endregion
    
    #region Private variables

    private PJ pj;
    private GameObject pjGO;
    private List<EnemyAI> enemies;
    private CameraDirector cameraDirector;
    private bool gameIn3D;
    private bool isInitialLoad = true;
    private bool isFirstFloorLoad = true;
    private bool timeLoopEnded;
    private float initialTimeLoopDuration;
    private float secondsCounter = 0;
    private bool pjCameFromAbove;
    private bool isNewCycleOrLoop = true;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private SceneDirector sceneDirector;
    private Vector2 inputDirection = Vector2.zero;

    //INPUT ACTIONS
    private InputAction MoveAction;
    private InputAction RollAction;
    private InputAction InteractAction;
    private InputAction CameraChangeAction;
    private InputAction CameraRotationAction;

    private static bool IsSceneT1C0F0 => SceneManager.GetActiveScene().name == "T1C0F0";
    private static bool IsSceneT1C1Fm1 => SceneManager.GetActiveScene().name == "T1C1F-1";
    
    private Dictionary<string,FloorData> loopPersistentData;
    
    private struct FloorData
    {
        public bool enemiesDefeated;
        public Dictionary<string,DialogueSO> NPCsDialogues;
    }

    #endregion

    #region Unity Events
    
    private void Start()
    {
        initialTimeLoopDuration = timeLoopDuration;

        if (isInitialLoad)
        {
            if (!debugMode)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }

            var moonRotation = moon.transform.rotation;

            sceneDirector = transform.parent.GetComponentInChildren<SceneDirector>();
            sceneDirector.DoStart();

            Core.Dialogue.Initialize(canvas);

            this.EventSubscribe<GameEvents.EnemyDied>(e => EnemyDied(e.enemy));
            this.EventSubscribe<GameEvents.NPCVanished>(e => ShowGodNarrative());
            this.EventSubscribe<GameEvents.DoorOpened>(e => DoorOpened());
            this.EventSubscribe<GameEvents.PlayerDamaged>(e => PlayerDamaged());
            
            this.EventSubscribe<GameEvents.LoadFloorSceneEvent>(e =>
            {
                pjCameFromAbove = e.toFloorBelow;
                sceneDirector.LoadNewFloorScene(e.toFloorBelow);
            });
            this.EventSubscribe<GameEvents.LoadInitialFloorSceneEvent>(e => sceneDirector.LoadInitialFloor());
            
            this.EventSubscribe<GameEvents.NPCDialogueEnded>(e => UpdateCurrentFloorEndedNPCDialogue(e.npc, e.lastDialogue));
            
            UpdateGameState();

            Core.Audio.Initialize(audioGO);
            Core.Audio.Play(SOUND_TYPE.BackgroundMusic, 1f, 0.2f);
            
            isInitialLoad = false;
        }
        
        if (playerInput)
        {
            MoveAction = playerInput.actions["Move"];
            RollAction = playerInput.actions["Roll"];
            InteractAction = playerInput.actions["Action"];
            CameraChangeAction = playerInput.actions["CameraChange"];
            CameraRotationAction = playerInput.actions["CameraRotation"];
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

    private void EnemyDied(EnemyAI defeatedEnemy)
    {
        enemies.Remove(defeatedEnemy);
        CheckEnemiesInScene();
    }

    private void UpdateCurrentFloorEndedNPCDialogue(NPC npc, DialogueSO lastDialogue)
    {
        FloorData currentFloorData = loopPersistentData[SceneManager.GetActiveScene().name];
        if (!currentFloorData.NPCsDialogues.ContainsKey(npc.name))
        {
            currentFloorData.NPCsDialogues.Add(npc.name, lastDialogue);
            loopPersistentData[SceneManager.GetActiveScene().name] = currentFloorData;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        StartCoroutine(WaitAndInitializeGameDirector());
    }
    
    void Update()
    {
        if (cameraDirector != null && !cameraDirector.CamerasTransitionBlending() && (!timeLoopEnded || debugMode))
        {
            if (debugMode && CameraChangeAction.triggered)
            {
                SwitchGamePerspective();
            }
            else if (pj != null && !narrativeDirector.IsShowingNarrative())
            {
                // Player
                ControlInputData controlInputData = GetControlInputDataValues();
                
                pj.DoUpdate(controlInputData);

                if (enemies != null)
                {
                    foreach (EnemyAI enemy in enemies)
                    {
                        enemy.DoUpdate();
                    }
                }
                
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
                EndTimeLoop();
            }
        }
    }

    #endregion
    
    #region Initializacion

    private IEnumerator WaitAndInitializeGameDirector()
    {
        yield return null;

        InitializeGameDirector();
        
        if (IsSceneT1C0F0)
        {
            StartT1C0F0GameFlow();
        }
        else
        {
            CheckEnemiesInScene();
        }
    }

    private void InitializeGameDirector()
    {
        InitializePlayer();
        InitializeCameraDirector();
        SetPlayerSpawnPoint();

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (isNewCycleOrLoop)
        {
            loopPersistentData = new Dictionary<string, FloorData>();
        }

        if (!loopPersistentData.ContainsKey(currentSceneName))
        {
            FloorData newFloorData = new FloorData();
            newFloorData.enemiesDefeated = false;
            newFloorData.NPCsDialogues = new Dictionary<string, DialogueSO>();

            loopPersistentData.Add(currentSceneName, newFloorData);
        }
        
        enemies = FindObjectsOfType<EnemyAI>().ToList();
        
        FloorData currentFloorData;
        loopPersistentData.TryGetValue(currentSceneName, out currentFloorData);

        List<EnemyAI> removedEnemies = new List<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            if (!currentFloorData.enemiesDefeated)
            {
                enemy.Initialize(pjGO.transform);
            }
            else
            {
                removedEnemies.Add(enemy);
                Destroy(enemy.transform.parent.gameObject);
            }
        }

        foreach (EnemyAI removedEnemy in removedEnemies)
        {
            enemies.Remove(removedEnemy);
        }
        
        NPC[] npcs = FindObjectsOfType<NPC>();

        foreach (NPC npc in npcs)
        {
            if (currentFloorData.NPCsDialogues.ContainsKey(npc.name))
            {
                DialogueSO lastNPCDialogue = currentFloorData.NPCsDialogues[npc.name];
                npc.dialogueEnded = true;
                npc.lastDialog = lastNPCDialogue;
            }
        }
        
        if (timeLoopEnded)
        {
            // Restart time loop
            timeLoopDuration = initialTimeLoopDuration;
            timeLoopEnded = false;
        }
        
        // When a new scene has been loaded, the new cycle or loop processing has been done
        isNewCycleOrLoop = false;
    }

    private void SetPlayerSpawnPoint()
    {
        GameObject[] pjSpawnPoints = GameObject.FindObjectsOfType<GameObject>()
            .Where(objeto => objeto.layer == Layers.PJ_SPAWN_LAYER).ToArray();
        if (!debugMode && isNewCycleOrLoop)
        {
            pj.transform.position = new Vector3(0, 0, 0);
        }
        else if (!debugMode && pjSpawnPoints != null && pjSpawnPoints.Length > 0)
        {
            Vector3 aboveSpawnPosition = new Vector3();
            Vector3 belowSpawnPosition = new Vector3();

            foreach (GameObject pjSpawnPoint in pjSpawnPoints)
            {
                if (pjSpawnPoint.name.Contains("Above"))
                {
                    aboveSpawnPosition = pjSpawnPoint.transform.position;
                }
                else if (pjSpawnPoint.name.Contains("Below"))
                {
                    belowSpawnPosition = pjSpawnPoint.transform.position;
                }
            }

            if (pjCameFromAbove)
            {
                pj.transform.position = aboveSpawnPosition;
            }
            else
            {
                pj.transform.position = belowSpawnPosition;
            }
        }
    }

    private void InitializePlayer()
    {
        PJ[] players = FindObjectsOfType<PJ>();

        foreach (PJ player in players)
        {
            if (pj == null)
            {
                pj = player;
                pjGO = pj.gameObject;
                pj.gameObject.transform.parent = transform.parent;
            }
            else if (!IsPersistentPlayer(player))
            {
                Destroy(player.gameObject);
            }
        }
    }
    
    private bool IsPersistentPlayer(PJ player)
    {
        return player.gameObject.transform.parent == transform.parent;
    }

    private void InitializeCameraDirector()
    {
        if (Camera.main != null)
        {
            cameraDirector = Camera.main.GetComponent<CameraDirector>();
            cameraDirector.Initialize(pj.transform);
        }
        else
        {
            cameraDirector = null;
        }
    }
    
    #endregion

    #region Gameflow methods

    private void StartT1C0F0GameFlow()
    {
        Core.PositionRecorder.StartRecording(pj.transform, moon.transform);
        Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C0F0"]);
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

    private void EndTimeLoop()
    {
        timeLoopEnded = true;
        
        if (!IsSceneT1C0F0 && !debugMode)
        {
            isFirstFloorLoad = true;
            isNewCycleOrLoop = true;
            pj.ResetItems();
            Core.Event.Fire<GameEvents.LoadInitialFloorSceneEvent>();
        }
        else if (IsSceneT1C0F0)
        {
            Core.PositionRecorder.StopRecording();
            Core.PositionRecorder.DoRewind(pj.transform, moon.transform, () => { StartCycle1(); });
        }
    }

    private void StartCycle1()
    {
        List<string> cycle1Floors = new List<string>(){"T1C1F0", "T1C1F-1"};
        int cycle1InitialFloor = 0;
        initialTimeLoopDuration = cycle1LoopDuration;
        isNewCycleOrLoop = true;
        Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C1F0"]);
        sceneDirector.setTowerFloorScenes(cycle1Floors, cycle1InitialFloor);
        sceneDirector.LoadCurrentFloorScene();
    }
    
    private void StartCycle2()
    {
        List<string> cycle2Floors = new List<string>(){"T1C2F1", "T1C2F0", "T1C2F-1", "T1C2F-2"};
        int cycle2InitialFloor = 1;
        timeLoopEnded = true;
        isNewCycleOrLoop = true;
        initialTimeLoopDuration = cycle2LoopDuration;
        sceneDirector.setTowerFloorScenes(cycle2Floors, cycle2InitialFloor);
        sceneDirector.LoadCurrentFloorScene();
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
        if (enemies.Count <= 0)
        {
            if (IsSceneT1C1Fm1)
            {
                StartCycle2();
            }
            else
            {
                FloorData currentScenePersistentData = loopPersistentData[SceneManager.GetActiveScene().name];
                currentScenePersistentData.enemiesDefeated = true;
                loopPersistentData[SceneManager.GetActiveScene().name] = currentScenePersistentData;
                
                SetGameState(true);
            }
        } else if (gameIn3D)
        {
            SetGameState(false);
        }
    }
    
    #endregion

    #region Utils

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