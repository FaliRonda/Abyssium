using Cinemachine;
using Puzzles;
using UnityEngine;

public class Chest : Interactable
{
    public CombinationPuzzleSO combinationPuzzleData;
    private PJ pj;
    [HideInInspector] public bool puzzleIsSolved;
    private CinemachineVirtualCamera puzzleCamera;

    public override void Interact(PJ pj)
    {
        puzzleCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        this.pj = pj;
        
        if (!IsInteracting())
        {
            SetInteracting(true);
            StartPuzzle();
        }
    }

    private void StartPuzzle()
    {
        SwtichToCameraToPuzzle(true);
        Core.Event.Fire(new GameEvents.StartCombinationPuzzle(){combinationPuzzleData = combinationPuzzleData, originInteractable = this});
    }

    public void SwtichToCameraToPuzzle(bool setPuzzleCamera)
    {
        if (setPuzzleCamera)
        {
            Core.CameraEffects.SetPJVisibility(false);
            puzzleCamera.Priority = 50;
        }
        else
        {
            Core.CameraEffects.SetPJVisibility(true);
            puzzleCamera.Priority = 0;
        }
    }

    public void PuzzleExit()
    {
        SwtichToCameraToPuzzle(false);
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
