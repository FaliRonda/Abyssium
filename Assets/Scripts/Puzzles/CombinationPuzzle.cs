using DG.Tweening;
using Ju.Extensions;
using Puzzles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombinationPuzzle : MonoBehaviour
{
    private CombinationPuzzleSO combinationPuzzleData;
    private Interactable originInteractable;
    private Button[] optionButtons;
    private int selectedButtonIndex;
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
        foreach (Button button in optionButtons)
        {
            button.gameObject.SetActive(true);
        }
    }
    
    private void HidePuzzleUI()
    {
        foreach (Button button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void InitializePuzzleUI()
    {
        chest = (Chest)originInteractable;
        
        int buttonsCount = transform.childCount;
        optionButtons = new Button[buttonsCount];
        selectedSymbolsIndex = new int[buttonsCount];
        
        for (int buttonIndex = 0; buttonIndex < buttonsCount; buttonIndex++)
        {
            GameObject currentButtonGO = transform.GetChild(buttonIndex).gameObject;
            currentButtonGO.SetActive(true);
            optionButtons[buttonIndex] = currentButtonGO.GetComponentInChildren<Button>();
            
            if (buttonIndex < buttonsCount - 1)
            {
                TMP_Text optionText = optionButtons[buttonIndex].GetComponentInChildren<TMP_Text>();
                optionText.text = combinationPuzzleData.symbolsDictionary[buttonIndex].symbols[selectedSymbolsIndex[buttonIndex]];
            }
            
        }
        
        optionButtons[selectedButtonIndex].GetComponentInChildren<Outline>().enabled = true;

        puzzleInitialized = true;
    }

    public void HandleInput(GameDirector.ControlInputData controlInputData, bool interactActionTriggered)
    {
        if (!chest.puzzleIsSolved)
        {
            UpdateSelectedButton(controlInputData.inputDirection);
            UpdateSelectedSymbol(controlInputData.inputDirection);

            CheckExit(interactActionTriggered);
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
                        optionButtons[selectedButtonIndex].GetComponentInChildren<Outline>().enabled = false;
                        chest.PuzzleSolved();
                    }) 
                    .AppendInterval(2f)
                    .AppendCallback(() =>
                    {
                        HidePuzzleUI();
                        Core.Event.Fire(new GameEvents.PuzzleSolved(){puzzleID = combinationPuzzleData.puzzleID});
                    });
            }
        }
    }

    private void UpdateSelectedSymbol(Vector3 inputDirection)
    {
        if (!symbolSelected && selectedButtonIndex < optionButtons.Length - 1)
        {
            // Update the current button's symbol index
            if (inputDirection.y == 1)
            {
                if (selectedSymbolsIndex[selectedButtonIndex] + 1 <
                    combinationPuzzleData.symbolsDictionary[selectedButtonIndex].symbols.Count)
                {
                    selectedSymbolsIndex[selectedButtonIndex]++;
                }
                else
                {
                    selectedSymbolsIndex[selectedButtonIndex] = 0;
                }

                symbolSelected = true;
            }
            else if (inputDirection.y == -1)
            {
                if (selectedSymbolsIndex[selectedButtonIndex] > 0)
                {
                    selectedSymbolsIndex[selectedButtonIndex]--;
                }
                else
                {
                    selectedSymbolsIndex[selectedButtonIndex] =
                        combinationPuzzleData.symbolsDictionary[selectedButtonIndex].symbols.Count - 1;
                }
                
                symbolSelected = true;
            }

            if (selectedButtonIndex < optionButtons.Length - 1)
            {
                optionButtons[selectedButtonIndex].GetComponentInChildren<TMP_Text>().text =
                    combinationPuzzleData.symbolsDictionary[selectedButtonIndex].symbols[selectedSymbolsIndex[selectedButtonIndex]];
            }
        } else if (inputDirection.y == 0)
        {
            symbolSelected = false;
        }
    }

    private void CheckExit(bool interactActionTriggered)
    {
        if (selectedButtonIndex == optionButtons.Length - 1 && interactActionTriggered)
        {
            HidePuzzleUI();
            
            chest.PuzzleExit();
            
            Core.Event.Fire(new GameEvents.PuzzlePaused(){});
        }
    }

    public void UpdateSelectedButton(Vector3 inputDirection)
    {
        if (!buttonSelected)
        {
            optionButtons[selectedButtonIndex].GetComponentInChildren<Outline>().enabled = false;
            
            if (inputDirection.x == 1 && (selectedButtonIndex + 1 < optionButtons.Length))
            {
                selectedButtonIndex++;
                buttonSelected = true;
            }
            else if (inputDirection.x == -1 && selectedButtonIndex > 0)
            {
                selectedButtonIndex--;
                buttonSelected = true;
            }
            
            optionButtons[selectedButtonIndex].GetComponentInChildren<Outline>().enabled = true;
        } else if (inputDirection.x == 0)
        {
            buttonSelected = false;
        }
    }
}
