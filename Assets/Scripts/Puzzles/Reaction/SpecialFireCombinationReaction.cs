using DG.Tweening;
using UnityEngine;

public class SpecialFireCombinationReaction : CombinationReaction
{
    public GameObject runes;
    public Color runesReactionColor;
    public GameObject magicPaints;
    public Light[] lights;
    public Color lightsReactionColor;
    public Light[] candles;
    public Color candlesReactionColor;
    

    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);

        DropPoint ritualDropPoint = (DropPoint)conversable;

        if (MagicBottleContainsFire(ritualDropPoint))
        {
            runes.GetComponent<EmissionGlow>().SetEmissionColor(runesReactionColor);
            runes.GetComponent<EmissionGlow>().UpdateValues(50, 0, 1.5f, 0, 2f);
            runes.GetComponent<EmissionGlow>().enabled = true;
            
            foreach (Light light in lights)
            {
                light.DOColor(lightsReactionColor, 0.2f);
            }
            
            foreach (Light candle in candles)
            {
                candle.DOColor(candlesReactionColor, 0.2f);
            }
            

            foreach (EmissionGlow emissionGlow in magicPaints.GetComponentsInChildren<EmissionGlow>())
            {
                emissionGlow.UpdateValues(50, -5, 1.5f, 0, 2f);
            }
            
            magicPaints.SetActive(true);
            
            reactionPerformed = true;
        }
    }

    private bool MagicBottleContainsFire(DropPoint dropPoint)
    {
        bool containsFire = false;
        foreach (Draggable droppedDraggable in dropPoint.droppedDraggables)
        {
            if (droppedDraggable.name == "MagicalBottle" &&
                droppedDraggable.GetComponentInChildren<Renderer>().materials[1].name == "MagicFireBottle (Instance)")
            {
                containsFire = true;
            }
        }

        return containsFire;
    }
}
