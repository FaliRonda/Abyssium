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
        public bool toFloorBelow;
    }

    public struct EnemyDied
    {
        public EnemyAI enemy;
    }

    public struct NPCVanished {}

    public struct DoorOpened {}

    public struct PlayerDamaged {}

    public struct NPCDialogueEnded
    {
        public NPC npc;
        public DialogueSO lastDialogue;
    }
}