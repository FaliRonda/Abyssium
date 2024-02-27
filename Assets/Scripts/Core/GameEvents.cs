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

    public struct NPCDialogue
    {
        public bool started;
    }

    public struct TryOpenLockedDoor {}

    public struct PlayerDamaged
    {
        public float deathFrameDuration;
    }

    public struct NPCDialogueEnded
    {
        public NPC npc;
        public DialogueSO lastDialogue;
    }

    public struct NPCMemoryGot {}

    public struct OrbGot {}

    public struct BossCombatReached {}

    public struct EnemySpawned
    {
        public EnemyAI enemyAI;
    }
}