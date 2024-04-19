using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Bloom = UnityEngine.Rendering.Universal.Bloom;

public class BlockSpecialDraggableReaction : SpecialDraggableInteractionReaction
{
    public float maxHeight;
    public float movementDuration;
    public Material dissolveMaterial;
    public Color dissolveColor;
    public CinemachineVirtualCamera runesCamera;
    public Volume postprocessing;
    public Light[] lights;
    
    private Bloom bloom;

    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);

        if (!reactionPerformed)
        {
            postprocessing.profile.TryGet<Bloom>(out bloom);
            
            GameState.controlBlocked = true;
            reactionPerformed = true;
            
            transform.position = conversable.transform.position;
            
            gameObject.SetActive(true);
            runesCamera.Priority = 50;
            
            Renderer boxRenderer = GetComponentInChildren<Renderer>();

            Sequence blockMovementSequence = DOTween.Sequence();

            Transform key = transform.GetChild(2);

            transform.DORotate(new Vector3(0, 360, 0), movementDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(-1);

            Core.CameraEffects.SetPJVisibility(false);
            
            blockMovementSequence
                .AppendInterval(1f)
                .Append(runesCamera.transform.DOMoveY(maxHeight + 1, movementDuration).SetEase(Ease.InOutQuad))
                .Join(transform.DOMoveY(maxHeight, movementDuration).SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        Color originalBoxColor = boxRenderer.materials[1].color;
                        Color originalLightColor = lights[0].color;

                        foreach (Light light in lights)
                        {
                            light.DOColor(originalBoxColor, 0.2f);
                        }
                            
                        Color originalBloomColor = Color.clear;
                        if (bloom != null)
                        {
                            originalBloomColor = bloom.tint.value;
                            bloom.tint.value = dissolveColor;
                            
                            DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, 2f, 0.2f)
                                .SetEase(Ease.OutQuad)
                                .OnComplete(() =>
                                {
                                    DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, 0.05f,
                                            0.2f)
                                        .SetEase(Ease.OutQuad);
                                });
                        }
                        
                        boxRenderer.materials[1]
                            .DOColor(dissolveColor, 1.5f)
                            .OnComplete(() =>
                            {
                                Material[] newMaterials = boxRenderer.materials;
                                newMaterials[1] = dissolveMaterial;
                                boxRenderer.materials = newMaterials;

                                boxRenderer.materials[1].SetColor("_ColorDissolve", dissolveColor);
                                boxRenderer.materials[1].SetFloat("_DissolveAmount", 1.8f);

                                DOTween.To(() => boxRenderer.materials[1].GetFloat("_DissolveAmount"), x =>
                                {
                                    boxRenderer.materials[1].SetFloat("_DissolveAmount", x);
                                }, 0, 1f)
                                .OnComplete(() =>
                                {
                                    if (bloom != null)
                                    {
                                        bloom.tint.value = originalBloomColor;
                                        
                                        foreach (Light light in lights)
                                        {
                                            light.DOColor(originalLightColor, 0.2f);
                                        }
                                    }

                                    key.parent = transform.parent.parent;
                                    gameObject.SetActive(false);
                                    key.DOJump(new Vector3(key.position.x, 0, key.position.z + 3), 1, 1, 2f)
                                        .OnComplete(() =>
                                        {
                                            key.GetComponent<Collider>().enabled = true;
                                            key.GetComponent<Draggable>().SetCanBeDraggable(true);
                                            
                                            DropPoint runesDropPoint = (DropPoint)conversable;
                                            runesDropPoint.pj.currentDraggable = null;
                                            
                                            runesCamera.Priority = 0;
                                            GameState.controlBlocked = false;
                                            Core.CameraEffects.SetPJVisibility(true);
                                        });
                                });
                                
                            });
                    }));
        }
    }
}