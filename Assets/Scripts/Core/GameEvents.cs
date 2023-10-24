using UnityEngine.Serialization;

public static class GameEvents
{
    public struct SwitchPerspectiveEvent
    {
        public bool gameIn3D;
    }
    
    public struct LoadInitialFloorSceneEvent {}
    
    public struct LoadFloorSceneEvent
    {
        [FormerlySerializedAs("isFloorAbove")] public bool isFloorBelow;
    }
}