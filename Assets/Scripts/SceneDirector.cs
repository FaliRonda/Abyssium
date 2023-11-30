using System.Collections.Generic;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDirector : MonoBehaviour
{
    public int initialFloor;
    public List<string> towerFloors;
    
    private int currentFloor;

    public void DoStart()
    {
        currentFloor = initialFloor;
        this.EventSubscribe<GameEvents.LoadFloorSceneEvent>(e => LoadNewFloorScene(e.isFloorBelow));
        this.EventSubscribe<GameEvents.LoadInitialFloorSceneEvent>(e =>
        {
            currentFloor = initialFloor;
            LoadCurrentFloorScene();
        });
    }

    public void LoadCurrentFloorScene()
    {
        SceneManager.LoadScene(towerFloors[currentFloor]);
    }

    private void LoadNewFloorScene(bool isFloorBelow)
    {
        currentFloor = isFloorBelow ? currentFloor + 1 : currentFloor - 1;
        LoadCurrentFloorScene();
    }

    public void setTowerFloorScenes(List<string> cicle1Floors, int newInitialFloor)
    {
        towerFloors = cicle1Floors;
        initialFloor = newInitialFloor;
    }
}
