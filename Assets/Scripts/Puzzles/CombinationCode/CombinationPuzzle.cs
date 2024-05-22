using Cinemachine;
using DG.Tweening;
using FMOD.Studio;
using Ju.Extensions;
using Puzzles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombinationPuzzle : MonoBehaviour
{
    private CombinationPuzzleSO combinationPuzzleData;
    private Interactable originInteractable;
    private Image[] optionImages;
    private Image[] optionArrows;
    private int selectedImageIndex;
    private int[] selectedSymbolsIndex;
    private bool symbolSelected;
    private bool buttonSelected;
    private bool puzzleInitialized;
    private Chest chest;

    private void Start()
    {
        this.EventSubscribe<GameEvents.StartCombinationPuzzle>(e => RunCombinationPuzzle(e.combinationPuzzleData, e.originInteractable));
    }

    private void RunCombinationPuzzle(CombinationPuzzleSO combinationPuzzleData, Interactable originInteractable)
    {
        if (!puzzleInitialized)
        {
            this.combinationPuzzleData = combinationPuzzleData;
            this.originInteractable = originInteractable;
            
            chest = (Chest)originInteractable;

            InitializePuzzleUI();
        }
        else
        {
            ShowPuzzleUI();
        }

        Core.Event.Fire(new GameEvents.PuzzleRunning(){puzzle = this});
    }

    private void ShowPuzzleUI()
    {
        foreach (Image button in optionImages)
        {
            button.gameObject.SetActive(true);
        }
        
        optionArrows[selectedImageIndex].gameObject.SetActive(true);
    }
    
    private void HidePuzzleUI()
    {
        foreach (Image button in optionImages)
        {
            button.gameObject.SetActive(false);
        }

        optionImages[selectedImageIndex].GetComponentInChildren<Outline>().enabled = false;
        
        if (selectedImageIndex < optionArrows.Length - 1)
        {
            optionArrows[selectedImageIndex].gameObject.SetActive(false);
        }

        selectedImageIndex = 0;
    }

    private void InitializePuzzleUI()
    {
        int buttonsCount = transform.GetChild(0).childCount;
        optionImages = new Image[buttonsCount];
        optionArrows = new Image[buttonsCount];
        selectedSymbolsIndex = new int[buttonsCount];
        
        for (int buttonIndex = 0; buttonIndex < buttonsCount; buttonIndex++)
        {
            GameObject currentImageGO = transform.GetChild(0).GetChild(buttonIndex).gameObject;

            currentImageGO.SetActive(true);
            optionImages[buttonIndex] = currentImageGO.GetComponentInChildren<Image>();

            if (buttonIndex < buttonsCount - 1)
            {
                GameObject currentArrowsGO = transform.GetChild(1).GetChild(buttonIndex).gameObject;
                optionArrows[buttonIndex] = currentArrowsGO.GetComponentInChildren<Image>();
                
                TMP_Text optionText = optionImages[buttonIndex].GetComponentInChildren<TMP_Text>();
                optionText.text = combinationPuzzleData.symbolsDictionary[buttonIndex].symbols[selectedSymbolsIndex[buttonIndex]];
            }
            
        }
        
        optionImages[selectedImageIndex].GetComponentInChildren<Outline>().enabled = true;
        optionArrows[selectedImageIndex].gameObject.SetActive(true);

        puzzleInitialized = true;
    }

    public void HandleInput(PlayerInputService.ControlInputData controlInputData)
    {
        if (!chest.puzzleIsSolved)
        {
            UpdateSelectedImage(controlInputData.inputDirection);
            UpdateSelectedSymbol(controlInputData.inputDirection);

            CheckExit(Core.PlayerInput.ActionTriggered(PlayerInputService.ACTION_TYPE.INTERACT));
        }

        CheckSolution();
    }

    private void CheckSolution()
    {
        if (!chest.puzzleIsSolved)
        {
            bool combinationIsCorrect = true;
            
            for (int symbolIndex = 0; symbolIndex < combinationPuzzleData.solutionCombination.Length; symbolIndex++)
            {
                string solutionSymbol = combinationPuzzleData.solutionCombination[symbolIndex];
                string currentSymbol = combinationPuzzleData.symbolsDictionary[symbolIndex]
                        .symbols[selectedSymbolsIndex[symbolIndex]];
                if (solutionSymbol != currentSymbol)
                {
                    combinationIsCorrect = false;
                } 
            }
            if (combinationIsCorrect)
            {
                Sequence puzzleSolvedSequence = DOTween.Sequence();
                puzzleSolvedSequence
                    .AppendCallback(() =>
                    {
                        //sonido
                        optionImages[selectedImageIndex].GetComponentInChildren<Outline>().enabled = false;
                        chest.PuzzleSolved();
                    }) 
                    .AppendInterval(2f)
                    .AppendCallback(() =>
                    {
                        chest.SwtichToCameraToPuzzle(false);
                        HidePuzzleUI();
                        Core.Event.Fire(new GameEvents.PuzzleSolved(){puzzleID = combinationPuzzleData.puzzleID});
                    });
            }
        }
    }

    private void UpdateSelectedSymbol(Vector3 inputDirection)
    {
        if (!symbolSelected && selectedImageIndex < optionImages.Length - 1)
        {
            if (inputDirection.y != 0)
            {
                Core.Audio.PlayFMODAudio("event:/Puzzle/Chest/SymbolUpdate", transform);
            }
            // Update the current button's symbol index
            if (inputDirection.y == 1)
            {
                if (selectedSymbolsIndex[selectedImageIndex] + 1 <
                    combinationPuzzleData.symbolsDictionary[selectedImageIndex].symbols.Count)
                {
                    selectedSymbolsIndex[selectedImageIndex]++;
                }
                else
                {
                    selectedSymbolsIndex[selectedImageIndex] = 0;
                }

                symbolSelected = true;
            }
            else if (inputDirection.y == -1)
            {
                if (selectedSymbolsIndex[selectedImageIndex] > 0)
                {
                    selectedSymbolsIndex[selectedImageIndex]--;
                }
                else
                {
                    selectedSymbolsIndex[selectedImageIndex] =
                        combinationPuzzleData.symbolsDictionary[selectedImageIndex].symbols.Count - 1;
                }
                
                symbolSelected = true;
            }

            if (selectedImageIndex < optionImages.Length - 1)
            {
                optionImages[selectedImageIndex].GetComponentInChildren<TMP_Text>().text =
                    combinationPuzzleData.symbolsDictionary[selectedImageIndex].symbols[selectedSymbolsIndex[selectedImageIndex]];
            }
        } else if (inputDirection.y == 0)
        {
            symbolSelected = false;
        }
    }

    private void CheckExit(bool interactActionTriggered)
    {
        if (selectedImageIndex == optionImages.Length - 1 && interactActionTriggered)
        {
            HidePuzzleUI();
            
            chest.PuzzleExit();
            
            Core.Event.Fire(new GameEvents.PuzzlePaused(){});
        }
    }

    public void UpdateSelectedImage(Vector3 inputDirection)
    {
        if (!buttonSelected)
        {
            optionImages[selectedImageIndex].GetComponentInChildren<Outline>().enabled = false;

            if (selectedImageIndex < optionArrows.Length - 1)
            {
                optionArrows[selectedImageIndex].gameObject.SetActive(false);
            }
            
            if (inputDirection.x == 1 && (selectedImageIndex + 1 < optionImages.Length))
            {
                Core.Audio.PlayFMODAudio("event:/Puzzle/Chest/PositionUpdate", transform);
                selectedImageIndex++;
                buttonSelected = true;
            }
            else if (inputDirection.x == -1 && selectedImageIndex > 0)
            {
                Core.Audio.PlayFMODAudio("event:/Puzzle/Chest/PositionUpdate", transform);
                selectedImageIndex--;
                buttonSelected = true;
            }
            
            optionImages[selectedImageIndex].GetComponentInChildren<Outline>().enabled = true;
            
            if (selectedImageIndex < optionArrows.Length - 1)
            {
                optionArrows[selectedImageIndex].gameObject.SetActive(true);
            }
        }
        else if (inputDirection.x == 0)
        {
            buttonSelected = false;
        }
    }
}
