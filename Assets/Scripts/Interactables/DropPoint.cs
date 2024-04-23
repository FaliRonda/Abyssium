using System.Collections.Generic;
using Ju.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class DropPoint : Conversable
{
    public List<Draggable> dropableDraggables;
    public List<Transform> dropPoints;

    [System.Serializable]
    public class SpecialDraggablesDictionary : SerializableDictionary<Draggable, SpecialDraggableInteractionReaction> { }
    [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Draggable", ValueLabel = "Reaction")]
    public SpecialDraggablesDictionary specialDraggablesDictionary = new SpecialDraggablesDictionary();
    
    private CombinationReaction[] combinationReactions;
    [HideInInspector] public List<Draggable> droppedDraggables;
    private bool[] availableDropPoints;
    
    private DialogueSO dropQuestion;
    private DialogueSO dropFull;
    private Draggable currentDraggable;
    [HideInInspector] public PJ pj;
    private CombinationReaction currentCombinationReaction;

    protected override void Start()
    {
        base.Start();

        loopContent = true;
        
        droppedDraggables = new List<Draggable>();
        availableDropPoints = new bool[dropPoints.Count];

        for (int i = 0; i < dropPoints.Count; i++)
        {
            droppedDraggables.Add(null);
            availableDropPoints[i] = true;
        }
        
        dropQuestion = Resources.Load<DialogueSO>("Narrative/Puzzle prototype/DraggableChoices/DropQuestion");
        dropFull = Resources.Load<DialogueSO>("Narrative/Puzzle prototype/DraggableChoices/DropFull");
        
        combinationReactions = GetComponents<CombinationReaction>();
        
        this.EventSubscribe<GameEvents.PlayerCarryDraggable>(e => CheckIfDraggableWasTook(e.grabbedDraggable, e.replacedDraggable));
    }

    private void CheckIfDraggableWasTook(Draggable grabbedDraggable, Draggable replacedDraggable)
    {
        if (droppedDraggables.Contains(grabbedDraggable))
        {
            if (currentCombinationReaction != null)
            {
                currentCombinationReaction.RevertReaction(this);
                currentCombinationReaction = null;
            }
            
            if (replacedDraggable == null)
            {
                availableDropPoints[droppedDraggables.IndexOf(grabbedDraggable)] = true;
                droppedDraggables[droppedDraggables.IndexOf(grabbedDraggable)] = null;
            }
            else
            {
                droppedDraggables[droppedDraggables.IndexOf(grabbedDraggable)] = replacedDraggable;
                CheckSpecialCombination();
            }
        }
    }

    public override void Interact(PJ pj, bool cancel)
    {
        this.pj = pj;
        bool specialReactionExecuted = false;

        if (!dialoguesExtended && pj.currentDraggable != null)
        {
            if (dropableDraggables.Contains(pj.currentDraggable))
            {
                currentDraggable = pj.currentDraggable;

                ExtendDialogues(dropQuestion);
            }

            foreach (KeyValuePair<Draggable,SpecialDraggableInteractionReaction> keyValuePair in specialDraggablesDictionary)
            {
                if (keyValuePair.Key == pj.currentDraggable)
                {
                    keyValuePair.Value.DoReaction(this);
                    specialReactionExecuted = true;
                    // Aquí estaría bien hacer un extend del diálogo con la pregunta, guardar el reaction en una variable,
                    // y en la respuesta "yes" comprobar si existe un especialReaction y hacerlo - ahora mismo habla y hace
                    // el efecto a la vez
                }
            }
        }

        if (!specialReactionExecuted)
        {
            base.Interact(pj, cancel);
        }
    }

    public override void ChoiceSelected(int choiceIndex)
    {
        if (choiceIndex == 0)
        {
            int dropPointIndex = 0;
            bool dropped = false;
            
            if (droppedDraggables.Contains(null))
            {
                foreach (bool availableDropPoint in availableDropPoints)
                {
                    if (!dropped)
                    {
                        if (availableDropPoint)
                        {
                            availableDropPoints[dropPointIndex] = false;
                            dropped = true;
                        }
                        else
                        {
                            dropPointIndex++;
                        }
                    }
                }

                if (dropped)
                {
                    // Quitar el objeto UI
                    
                    currentDraggable.transform.position = dropPoints[dropPointIndex].position;
                    currentDraggable.gameObject.SetActive(true);
                    
                    droppedDraggables[dropPointIndex] = pj.currentDraggable;

                    pj.currentDraggable = null;

                    CheckSpecialCombination();
                }
                
                ShowNextDialog();
            }
            else
            {
                ShowExtraDialog(dropFull);
            }
        }
        else
        {
            ShowNextDialog();
        }
    }

    private void CheckSpecialCombination()
    {
        if (AllDropPointsUsed())
        {
            foreach (CombinationReaction combinationReaction in combinationReactions)
            {
                bool combinationMet = true;
                foreach (Draggable draggable in droppedDraggables)
                {
                    combinationMet &= combinationReaction.combinationDraggables.Contains(draggable);
                }

                if (combinationMet)
                {
                    currentCombinationReaction = combinationReaction;
                    combinationReaction.DoReaction(this);
                }
            }
        }
    }

    private bool AllDropPointsUsed()
    {
        foreach (bool availableDropPoint in availableDropPoints)
        {
            if (availableDropPoint)
            {
                return false;
            }
        }

        return true;
    }

    public void ResetDropPoint()
    {
        for (int i = 0; i < dropPoints.Count; i++)
        {
            droppedDraggables[i] = null;
            availableDropPoints[i] = true;
        }
    }
}
