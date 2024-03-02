using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Ju.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Bloom = UnityEngine.Rendering.Universal.Bloom;
using ChromaticAberration = UnityEngine.Rendering.Universal.ChromaticAberration;
using InputAction = UnityEngine.InputSystem.InputAction;
using Vignette = UnityEngine.Rendering.Universal.Vignette;

public class GameDirector : MonoBehaviour
{
    #region Public variables
    
    public bool debugMode;
    public float timeLoopDuration = 10f;
    public GameObject moon;
    public Canvas canvas;
    public Volume postprocessing;
    public NarrativeDirector narrativeDirector;
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
        public Vector2 cameraMouseRotation;

        public ControlInputData(Vector3 movementDirection, Vector3 inputDirection, Vector2 cameraRotation, Vector2 cameraMouseRotation)
        {
            this.movementDirection = movementDirection;
            this.inputDirection = inputDirection;
            this.cameraRotation = cameraRotation;
            this.cameraMouseRotation = cameraMouseRotation;
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
    private bool timeLoopEnded;
    private bool timeLoopPaused;
    private float initialTimeLoopDuration;
    private float secondsCounter = 0;
    private bool pjCameFromAbove;
    private bool pjCameFromDoor;
    private bool isNewCycleOrLoop = true;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private SceneDirector sceneDirector;
    private Vector2 inputDirection = Vector2.zero;


    //INPUT ACTIONS
    private InputAction MoveAction;
    private InputAction RollAction;
    private InputAction InteractAction;
    private InputAction CameraChangeAction;
    private InputAction CameraRotationAction;
    private InputAction CameraRotationMouseAction;
    private InputAction CloseAction;

    private static bool IsSceneGameLoader => SceneManager.GetActiveScene().name == "GameLoader";
    private static bool IsSceneT1C0F0 => SceneManager.GetActiveScene().name == "T1C0F0";
    private static bool IsSceneT1C1F0 => SceneManager.GetActiveScene().name == "T1C1F0";
    private static bool IsSceneT1C1Fm1 => SceneManager.GetActiveScene().name == "T1C1F-1";
    private static bool IsSceneT1C1IT2F0 => SceneManager.GetActiveScene().name == "T1C1IT2F0";
    private static bool IsSceneT1C1IT2Fm1 => SceneManager.GetActiveScene().name == "T1C1IT2F-1";
    private static bool IsSceneT1C2Fm2 => SceneManager.GetActiveScene().name == "T1C2F-2";
    private bool doorLockedAttemptOpen;
    private bool controlBlocked;
    private bool firstTimeDamaged;
    private bool neverDamaged = true;
    private bool bossDefeated;
    private bool demoEnded;
    
    private bool orbLateralDialogShown;

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
#if !UNITY_EDITOR
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
#endif
        initialTimeLoopDuration = timeLoopDuration;
        GameState.debugMode = debugMode;

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
            this.EventSubscribe<GameEvents.EnemySpawned>(e => EnemySpawned(e.enemyAI));
            this.EventSubscribe<GameEvents.NPCVanished>(e => EndDemo());
            this.EventSubscribe<GameEvents.NPCDialogue>(e => HandleConversation(e.started));
            this.EventSubscribe<GameEvents.NPCMemoryGot>(e => Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["MemoryGot"]));
            this.EventSubscribe<GameEvents.BossCombatReached>(e => StartBossCombat());
            this.EventSubscribe<GameEvents.OrbGot>(e =>
            {
                if (!orbLateralDialogShown)
                {
                    Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["OrbGot"]);
                    orbLateralDialogShown = true;
                }
            });
            this.EventSubscribe<GameEvents.TryOpenLockedDoor>(e =>
            {
                if (!doorLockedAttemptOpen)
                {
                    Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["LockedDoorAttemptOpen"]);
                    doorLockedAttemptOpen = true;
                }
            });
            this.EventSubscribe<GameEvents.PlayerDamaged>(e => PlayerDamaged(e.deathFrameDuration));
            
            this.EventSubscribe<GameEvents.LoadFloorSceneEvent>(e =>
            {
                pj.PlayIdle();
                pjCameFromDoor = true;
                controlBlocked = true;
                pjCameFromAbove = e.toFloorBelow;
                sceneDirector.LoadNewFloorScene(e.toFloorBelow);
            });
            this.EventSubscribe<GameEvents.LoadInitialFloorSceneEvent>(e => sceneDirector.LoadInitialFloor());
            
            this.EventSubscribe<GameEvents.NPCDialogueEnded>(e => UpdateCurrentFloorEndedNPCDialogue(e.npc, e.lastDialogue));
            
            UpdateGameState();
            
            postprocessing.profile.TryGet(out vignette);
            postprocessing.profile.TryGet(out bloom);
            postprocessing.profile.TryGet(out chromaticAberration);

            if (IsSceneGameLoader)
            {
                vignette.intensity.value = 1;
                vignette.smoothness.value = 1;
                moon.GetComponentInChildren<Light>().enabled = false;
            }

            Core.Audio.Initialize(audioGO);
            //Core.Audio.Play(SOUND_TYPE.BackgroundMusic, 1, 0, 0.03f);
            Core.Audio.PlayFMODAudio("event:/Music/MVP_CombatDemoScene_Music", transform);
            
            isInitialLoad = false;
        }
        
        if (playerInput)
        {
            MoveAction = playerInput.actions["Move"];
            RollAction = playerInput.actions["Roll"];
            InteractAction = playerInput.actions["Action"];
            CameraChangeAction = playerInput.actions["CameraChange"];
            CameraRotationAction = playerInput.actions["CameraRotation"];
            CameraRotationMouseAction = playerInput.actions["CameraRotationMouse"];
            CloseAction = playerInput.actions["Close"];
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

    private void HandleConversation(bool conversationStarted)
    {
        if (conversationStarted)
        {
            timeLoopPaused = true;
        }
        else
        {
            timeLoopPaused = false;
        }
    }

    private void EnemyDied(EnemyAI defeatedEnemy)
    {
        enemies.Remove(defeatedEnemy);
        CheckEnemiesInScene(true);
    }

    private void EnemySpawned(EnemyAI spawnEnemy)
    {
        spawnEnemy.Initialize(pjGO.transform);
        enemies.Add(spawnEnemy);
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
        if (CloseAction.triggered)
        {
            Application.Quit();
        }
        if (!demoEnded && cameraDirector != null && !cameraDirector.CamerasTransitionBlending() && (!timeLoopEnded || debugMode))
        {
            if (debugMode && CameraChangeAction.triggered)
            {
                ForceSwitchGamePerspective();
            }
            else if (pj != null && !narrativeDirector.IsShowingNarrative && !controlBlocked)
            {
                // Player
                ControlInputData controlInputData = GetControlInputDataValues();
                
                pj.DoUpdate(controlInputData);

                if (enemies != null && !controlBlocked)
                {
                    foreach (EnemyAI enemy in enemies)
                    {
                        enemy.DoUpdate();
                    }
                }
                
                if (Core.Dialogue.ChoicesInScreen)
                {
                    Core.Dialogue.SelectChoicesWithControl(controlInputData.inputDirection);
                    if (InteractAction.triggered)
                    {
                        Core.Dialogue.ChoiceSelected(-1);
                    }
                }
                
                if (InteractAction.triggered)
                {
                    pj.DoMainAction();
                }
                
                if (RollAction.triggered)
                {
                    pj.DoRoll();
                }

            } else if (narrativeDirector.IsShowingNarrative && !narrativeDirector.IsTypingText && InteractAction.triggered)
            {
                narrativeDirector.EndNarrative();
                if (IsSceneT1C1IT2Fm1)
                {
                    Sequence godNarrativeEnded = DOTween.Sequence();
                    godNarrativeEnded.
                        AppendCallback(() => { Core.Audio.Play(SOUND_TYPE.Bell, 1,0, 0.01f); })
                        .AppendInterval(2f)
                        .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 1f)
                            .SetEase(Ease.OutQuad))
                        .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 1f, 1f)
                            .SetEase(Ease.OutQuad))
                        .AppendCallback(() =>
                        {
                            timeLoopPaused = false;
                            pj.ResetItems();
                            StartCycle2();
                        })
                        .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 1f)
                            .SetEase(Ease.OutQuad))
                        .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.5f, 1f)
                            .SetEase(Ease.OutQuad));
                }
            }
        }
        
        // If time loop is not disabled
        if (timeLoopDuration != -1)
        {
            if (timeLoopDuration > 0 && !timeLoopEnded && !timeLoopPaused)
            {
                timeLoopDuration -= Time.deltaTime;
                UpdateMoonRotation();

                secondsCounter += Time.deltaTime;
                if (secondsCounter >= 1)
                {
                    secondsCounter = 0;
                    float volume = 0.01f;

                    if (IsSceneT1C0F0)
                    {
                        volume = 0.03f;
                    }
                    
                    Core.Audio.Play(SOUND_TYPE.ClockTikTak, 2, 0, volume);
                }
            }
            else if (!timeLoopEnded && !timeLoopPaused)
            {
                if (IsSceneT1C2Fm2)
                {
                    Core.Audio.StopAll();
                    Core.Audio.Play(SOUND_TYPE.BackgroundMusic, 1, 0, 0.03f);
                }
                
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
        else if (IsSceneT1C1F0)
        {
            CheckEnemiesInScene(true);
        }
        else
        {
            if (IsSceneT1C2Fm2)
            {
                Core.Audio.StopAll();
            }
            CheckEnemiesInScene(false);
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

        if (IsSceneT1C1IT2F0)
        {
            pj.canRoll = true;
        }

        if (pjCameFromDoor)
        {
            controlBlocked = false;
            pjCameFromDoor = false;

            pj.Rotate180();
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
            Core.CameraEffects.Initialize(cameraDirector.cameraTD, cameraDirector.camera3D);
            Core.GamepadVibrationService.Initialize();
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

        Sequence vignetteSequence = DOTween.Sequence();
        Sequence smoothnessSequence = DOTween.Sequence();

        timeLoopPaused = true;
        controlBlocked = true;

        vignetteSequence.AppendInterval(2f)
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))
            .AppendInterval(1f)
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.55f, 1f)
                .SetEase(Ease.OutQuad));
        
        smoothnessSequence.AppendInterval(2f)
            .Append(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))
            .AppendInterval(1f)
            .Append(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.55f, 1f)
                .SetEase(Ease.OutQuad))
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                moon.GetComponentInChildren<Light>().enabled = true;
                Core.Audio.Play(SOUND_TYPE.Spotlight, 1, 0, 0.03f);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                timeLoopPaused = false;
                controlBlocked = false;
                Core.PositionRecorder.StartRecording(pj.transform, moon.transform);
                Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C0F0"]);
                Core.Audio.Play(SOUND_TYPE.Bell, 1, 0, 0.01f);
            });
    }
    
    private void UpdateGameState()
    {
        GameState.gameIn3D = this.gameIn3D;
    }
    
    private void UpdateMoonRotation()
    {
        float pendingTimeLoopDurationPorcentage = timeLoopDuration / initialTimeLoopDuration;
        float nextLighthoyseYRotation = 360f * pendingTimeLoopDurationPorcentage * -1;
        
        Vector3 finalMoonAngles = new Vector3(moon.transform.eulerAngles.x, nextLighthoyseYRotation,
            moon.transform.eulerAngles.z);

        if (IsSceneT1C1Fm1 && firstTimeDamaged)
        {
            firstTimeDamaged = false;
            controlBlocked = true;
            timeLoopPaused = true;

            pj.PlayIdle();
            
            Sequence firstDamagedSequence = DOTween.Sequence();

            firstDamagedSequence
                .AppendInterval(2f)
                .AppendCallback(() => { Core.CameraEffects.ZoomOut(1); })
                .AppendInterval(2f)
                .AppendCallback(() => { moon.transform.DORotate(finalMoonAngles, 2f); })
                .AppendInterval(2)
                .AppendCallback(() => { Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C1F-1"]); })
                .AppendInterval(12)
                .AppendCallback(() => { Core.CameraEffects.ZoomIn(1); })
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    controlBlocked = false;
                    timeLoopPaused = false;
                });
        }
        else
        {
            moon.transform.DORotate(finalMoonAngles, 0.2f);
        }
    }

    private void EndTimeLoop()
    {
        timeLoopEnded = true;
        Core.Audio.Play(SOUND_TYPE.Bell, 1, 0, 0.01f);

        if (!IsSceneT1C0F0)
        {
            Sequence endTimeLoopSequence = DOTween.Sequence();
            endTimeLoopSequence
                .AppendInterval(3f)
                .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 2f)
                    .SetEase(Ease.OutQuad))
                .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 1f, 2f)
                    .SetEase(Ease.OutQuad))
                .AppendCallback(() => { EndLoopLogic(); })
                .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 1f)
                    .SetEase(Ease.OutQuad))
                .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.5f, 1f)
                    .SetEase(Ease.OutQuad));
        }
        else
        {
            EndLoopLogic();
        }
    }

    private void EndLoopLogic()
    {
        if (IsSceneT1C0F0)
        {
            Core.PositionRecorder.StopRecording();
            pj.PlayWalk();
            Core.PositionRecorder.DoRewind(pj.transform, moon.transform, () => { StartCycle1(); });
        }
        else if (IsSceneT1C1Fm1)
        {
            StartCycle1Iteration2();
            pj.ResetItems();
        }
        else if (!debugMode)
        {
            isNewCycleOrLoop = true;
            pj.ResetItems();
            Core.Event.Fire(new GameEvents.LoadInitialFloorSceneEvent());
        }
    }

    private void StartCycle1()
    {
        List<string> cycle1Floors = new List<string>(){"T1C1F0", "T1C1F-1"};
        int cycle1InitialFloor = 0;
        initialTimeLoopDuration = cycle1LoopDuration;
        isNewCycleOrLoop = true;
        Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C1F0"]);
        sceneDirector.SetTowerFloorScenes(cycle1Floors, cycle1InitialFloor);
        sceneDirector.LoadInitialFloor();
    }
    
    private void StartCycle1Iteration2()
    {
        List<string> cycle1IT2Floors = new List<string>(){"T1C1IT2F0", "T1C1IT2F-1"};
        int cycleInitialFloor = 0;
        initialTimeLoopDuration = cycle1LoopDuration;
        isNewCycleOrLoop = true;
        Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C1IT2F0"]);
        sceneDirector.SetTowerFloorScenes(cycle1IT2Floors, cycleInitialFloor);
        sceneDirector.LoadInitialFloor();
    }
    
    private void StartCycle2()
    {
        List<string> cycle2Floors = new List<string>(){"T1C2F1", "T1C2F0", "T1C2F-1", "T1C2F-2"};
        int cycle2InitialFloor = 1;
        timeLoopEnded = true;
        isNewCycleOrLoop = true;
        controlBlocked = false;
        initialTimeLoopDuration = cycle2LoopDuration;
        Core.Dialogue.ShowLateralDialogs(sceneLateralDialogs["T1C2F0"]);
        sceneDirector.SetTowerFloorScenes(cycle2Floors, cycle2InitialFloor);
        sceneDirector.LoadInitialFloor();
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

        return new ControlInputData(direction, inputDirection, CameraRotationAction.ReadValue<Vector2>(), CameraRotationMouseAction.ReadValue<Vector2>());
    }

    #endregion

    #region Event callbacks
    
    private void PlayerDamaged(float deathFrameDuration)
    {
        timeLoopPaused = true;
        controlBlocked = true;
        
        foreach (EnemyAI enemy in enemies)
        {
            enemy.ResetAINodes();
        }
        
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(deathFrameDuration).AppendCallback(() =>
        {
            timeLoopPaused = false;
            controlBlocked = false;
            //Core.Audio.Play(SOUND_TYPE.PjDamaged, 1, 0.1f, 0.1f);
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/GetDamage", pj.transform);
        });
        
        if (pj.inventory.HasWeapon || IsSceneT1C1Fm1)
        {
            timeLoopDuration -= 10;
        }
        else
        {
            timeLoopDuration -= 40;
        }

        if (neverDamaged)
        {
            neverDamaged = false;
            firstTimeDamaged = true;
        }
    }

    private void EndDemo()
    {
        demoEnded = true;
        timeLoopPaused = true;
        pj.PlayIdle();
        
        Sequence angryGodSequence = DOTween.Sequence();

        angryGodSequence
            .AppendCallback(() => { Core.Audio.Play(SOUND_TYPE.AngryGod, 1, 0, 0.1f); })
            .AppendCallback(() => { Core.CameraEffects.StartShakingEffect(2f, 0.2f, 2f); })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                narrativeDirector.ShowNarrative();
            });
    }
    
    private void StartBossCombat()
    {
        if (!bossDefeated)
        {
            controlBlocked = true;
            timeLoopPaused = true;

            pj.PlayIdle();
            Core.Audio.StopAll();

            Sequence bossSequence = DOTween.Sequence();

            bossSequence
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    Core.Audio.Play(SOUND_TYPE.BossDoorClosed, 1, 0, 0.05f);
                    Core.CameraEffects.StartShakingEffect(3, 0.2f, 1);
                    FindObjectOfType<BossDoor>().Appear();
                })
                .AppendInterval(2)
                .AppendCallback(() =>
                {
                    Core.CameraEffects.StartShakingEffect(3, 0.2f, 1);
                    Core.Audio.Play(SOUND_TYPE.AngryGod, 3, 0, 0.05f);
                })
                .AppendInterval(2)
                .AppendCallback(() =>
                {
                    Core.Audio.Play(SOUND_TYPE.BossMusic, 1, 0, 0.03f);
                    controlBlocked = false;
                    timeLoopPaused = false;
                    enemies[0].aIActive = true;
                });
        }
    }
    
    private void CheckEnemiesInScene(bool enemyDied)
    {
        if (enemies.Count <= 0)
        {
            if (IsSceneT1C1IT2Fm1)
            {
                timeLoopPaused = true;
                controlBlocked = true;
                Sequence angryGodSequence = DOTween.Sequence();

                angryGodSequence
                    .AppendCallback(() => { Core.Audio.Play(SOUND_TYPE.AngryGod, 1, 0, 0.1f); })
                    .AppendCallback(() => { Core.CameraEffects.StartShakingEffect(2f, 0.2f, 1f); })
                    .AppendInterval(2f)
                    .AppendCallback(() =>
                    {
                        pj.PlayIdle();
                        narrativeDirector.ShowNarrative();
                    });
            }
            else
            {
                FloorData currentScenePersistentData = loopPersistentData[SceneManager.GetActiveScene().name];
                currentScenePersistentData.enemiesDefeated = true;
                loopPersistentData[SceneManager.GetActiveScene().name] = currentScenePersistentData;

                if (IsSceneT1C2Fm2 && !bossDefeated)
                {
                    bossDefeated = true;
                    timeLoopPaused = true;
                    controlBlocked = true;
                    
                    pj.PlayIdle();
                    Core.Audio.StopAll();
                    
                    Sequence bossdefeated = DOTween.Sequence();

                    bossdefeated
                        .AppendCallback(() => { Core.Audio.Play(SOUND_TYPE.AngryGod, 2.5f, 0, 0.1f); })
                        .AppendCallback(() => { Core.CameraEffects.StartShakingEffect(1.75f, 0.2f, 2.5f); })
                        .AppendInterval(4)
                        .AppendCallback(() =>
                        {
                            Core.Audio.Play(SOUND_TYPE.BossDoorClosed, 1, 0, 0.05f);
                            Core.CameraEffects.StartShakingEffect(3, 0.2f, 1);
                            FindObjectOfType<BossDoor>().Disappear();
                        })
                        .AppendInterval(2)
                        .AppendCallback(() => { SetGameState(true, enemyDied); });
                }
                else if (!debugMode)
                {
                    SetGameState(true, enemyDied);
                }
                
            }
        } else if (gameIn3D)
        {
            SetGameState(false, enemyDied);
        }
    }

    #endregion

    #region Utils

    private void ForceSwitchGamePerspective()
    {
        SetGameState(!gameIn3D, false);
    }

    private void UpdateCameraPerspective(bool enemyDied)
    {
        if (!gameIn3D)
        {
            vignette.intensity.value = 0.55f;
            vignette.smoothness.value = 0.55f;
            Core.Event.Fire(new GameEvents.SwitchPerspectiveEvent() { gameIn3D = this.gameIn3D });
            //Core.Audio.Play(SOUND_TYPE.CameraChange, 1 ,0, 0.01f);
            Core.Audio.PlayFMODAudio("event:/IngameUI/Camera/CameraToBehind", transform);
        }
        else
        {
            if (enemyDied)
            {
                controlBlocked = true;
                pj.PlayIdle();
                
                Sequence vignetteSequence = DOTween.Sequence();

                vignetteSequence
                    .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.1f)
                        .SetEase(Ease.OutQuad))
                    .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.75f, 1f)
                        .SetEase(Ease.OutQuad))
                    .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.1f)
                        .SetEase(Ease.OutQuad))
                    .AppendCallback(() =>
                    {
                        Core.Event.Fire(new GameEvents.SwitchPerspectiveEvent() { gameIn3D = this.gameIn3D });
                        // Core.Audio.Play(SOUND_TYPE.CameraChange, 1 ,0, 0.01f);
                        Core.Audio.PlayFMODAudio("event:/IngameUI/Camera/CameraToBehind", transform);
                    })
                    .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 0.3f)
                        .SetEase(Ease.OutQuad))
                    .AppendCallback(() => { controlBlocked = false; });
            }
            else
            {
                Core.Event.Fire(new GameEvents.SwitchPerspectiveEvent() { gameIn3D = this.gameIn3D });
                // Core.Audio.Play(SOUND_TYPE.CameraChange, 1 ,0, 0.01f);
                Core.Audio.PlayFMODAudio("event:/IngameUI/Camera/CameraToTop", transform);
            }
        }
    }

    private void SetGameState(bool gameIn3D, bool enemyDied)
    {
        this.gameIn3D = gameIn3D;
        UpdateGameState();
        UpdateCameraPerspective(enemyDied);
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