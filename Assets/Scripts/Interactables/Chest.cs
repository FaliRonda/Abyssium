using Puzzles;
using UnityEngine;
using UnityEngine.Serialization;

public class Chest : Interactable
{
    [FormerlySerializedAs("combinationPuzzle")] public CombinationPuzzleSO combinationPuzzleData;
    private PJ pj;
    [FormerlySerializedAs("puzzleSolved")] public bool puzzleIsSolved;

    public override void Interact(PJ pj)
    {
        this.pj = pj;
        if (!IsInteracting())
        {
            SetInteracting(true);
            StartPuzzle();
        }
    }

    private void StartPuzzle()
    {
        Core.Event.Fire(new GameEvents.StartCombinationPuzzle(){combinationPuzzleData = combinationPuzzleData, originInteractable = this});
    }
    
    public void PuzzleExit()
    {
        SetInteracting(false);
        pj.SetDoingAction(false);
    }
    
    public void PuzzleSolved()
    {
        GetComponentInChildren<Animator>().Play("OpenChest");
        // Sonido

        puzzleIsSolved = true;
        SetInteracting(false);
        SetCanInteract(false);
        pj.SetDoingAction(false);
    }
}
