using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpecialFireCombinationReaction : CombinationReaction
{
    public GameObject runes;
    public Color runesReactionColor;
    public GameObject magicPaints;
    public Light[] lights;
    public Color lightsReactionColor;
    public Light[] candles;
    public Color candlesReactionColor;
    public Volume postprocessing;
    
    private Bloom bloom;
    private ChromaticAberration chromaticAberration;

    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);

        DropPoint ritualDropPoint = (DropPoint)conversable;

        if (MagicBottleContainsFire(ritualDropPoint))
        {
            runes.GetComponent<EmissionGlow>().SetEmissionColor(runesReactionColor);
            runes.GetComponent<EmissionGlow>().UpdateValues(50, 0, 1.5f, 0, 2f);
            runes.GetComponent<EmissionGlow>().UpdateAudioEvent("event:/Puzzle/Rituals/FireCombinationGlow");
            runes.GetComponent<EmissionGlow>().enabled = true;

            Core.Audio.PlayFMODAudio("event:/Puzzle/Rituals/FireCombinationSFX", transform);
            Core.Audio.PlayFMODAudio("event:/Puzzle/Rituals/FireCombinationMusic", transform);
            
            Core.Audio.UpdateFMODBackgroundVolume(0);
            
            postprocessing.profile.TryGet<Bloom>(out bloom);
            if (bloom != null)
            {
                DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, 4f, 0.4f)
                    .SetEase(Ease.InOutElastic)
                    .OnComplete(() =>
                    {
                        DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, 0.05f,
                                0.2f)
                            .SetEase(Ease.OutQuad);
                    });

                DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, 2f,
                        0.2f)
                    .SetEase(Ease.InOutElastic);
            }
            
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
                emissionGlow.UpdateValues(200, -5, 1.5f, 0, 2f);
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
