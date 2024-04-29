using DG.Tweening;
using UnityEngine;

public class NPC : Conversable
{
    public Material dissolveMaterial;
    public Color dissolveColor;
    
    private bool playOnNextDialogue;
    private bool playingAudio;

    protected override void Start()
    {
        base.Start();

        loopContent = false;
    }

    protected override void ShowNextDialog()
    {
        if (playingAudio && dialogueIndex == dialoguesToShow.Length)
        {
            Core.Audio.PlayFMODAudio("event:/Puzzle/NPC/Laugh", transform);
                
            Renderer renderer = interactableSprite.GetComponent<Renderer>();

            renderer.material = dissolveMaterial;
            renderer.material.SetColor("_ColorDissolve", dissolveColor);
            renderer.material.SetFloat("_DissolveAmount", 1.8f);

            SpriteRenderer[] NPCSprites = GetComponentsInChildren<SpriteRenderer>();
            NPCSprites[1].gameObject.SetActive(false);
            NPCSprites[2].gameObject.SetActive(false);
            
            DOTween.To(() => renderer.material.GetFloat("_DissolveAmount"), x =>
            {
                renderer.material.SetFloat("_DissolveAmount", x);
            }, 0, 3f);
            
            Core.Event.Fire(new GameEvents.NPCVanished());
        }
        
        if (playOnNextDialogue && !playingAudio)
        {
            Core.Audio.PlayFMODAudio("event:/Puzzle/NPC/End", transform);
            playingAudio = true;
        }
        
        if (dialogueIndex < dialoguesToShow.Length && dialoguesToShow[dialogueIndex].playSuspenseMusic)
        {
            Core.Audio.StopAllFMODAudios();
            playOnNextDialogue = true;
        }
        
        base.ShowNextDialog();
    }
}
