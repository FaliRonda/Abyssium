using Puzzles;
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
        public bool isBoss;
    }

    public struct NPCVanished {}

    public struct ConversableDialogue
    {
        public bool started;
    }

    public struct TryOpenLockedDoor {}

    public struct PlayerDamaged
    {
        public float deathFrameDuration;
    }

    public struct ConversableDialogueEnded
    {
        public Conversable conversable;
        public DialogueSO lastDialogue;
    }

    public struct NPCMemoryGot {}

    public struct OrbGot {}

    public struct BossCombatReached {}

    public struct EnemySpawned
    {
        public EnemyAI enemyAI;
    }

    public struct BossDied {}

    public struct BossSpawned
    {
        public int bossLife;
    }

    public struct BossDamaged {}

    public struct StartCombinationPuzzle
    {
        public CombinationPuzzleSO combinationPuzzleData;
        public Interactable originInteractable;
    }

    public struct PuzzleRunning
    {
        public CombinationPuzzle puzzle;
    }

    public struct PuzzlePaused
    {
    }

    public struct PuzzleSolved
    {
        public int puzzleID;
    }
}