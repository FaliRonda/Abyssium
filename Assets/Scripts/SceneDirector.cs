using System.Collections.Generic;
using Ju.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDirector : MonoBehaviour
{
    public int initialFloor;
    public List<SceneAsset> towerFloors;
    
    private int currentFloor;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        currentFloor = initialFloor;
        this.EventSubscribe<GameEvents.LoadFloorSceneEvent>(e => LoadNewFloorScene(e.isFloorBelow));
        this.EventSubscribe<GameEvents.LoadInitialFloorSceneEvent>(e =>
        {
            currentFloor = initialFloor;
            LoadCurrentFloorScene();
        });
    }

    private void LoadCurrentFloorScene()
    {
        SceneManager.LoadScene(towerFloors[currentFloor].name);
    }

    private void LoadNewFloorScene(bool isFloorBelow)
    {
        currentFloor = isFloorBelow ? currentFloor + 1 : currentFloor - 1;
        LoadCurrentFloorScene();
    }
}
