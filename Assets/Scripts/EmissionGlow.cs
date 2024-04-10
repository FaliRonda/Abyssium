using UnityEngine;
using DG.Tweening;

public class EmissionGlow : MonoBehaviour
{
    public float maxIntensity = 1f;
    public float minIntensity = 0f;
    public float transitionDuration = 1f;
    public float timeLightDuration = 1f;
    public float timeDarkDuration = 1f;

    private Material material;
    private Color emissionColor = Color.magenta;
    
    private void Start()
    {
        foreach (Material currentMaterial in GetComponent<Renderer>().materials)
        {
            if (currentMaterial.HasProperty("_EmissionColor"))
            {
                material = currentMaterial;
            }
        }
        
        if (material == null || !material.HasProperty("_EmissionColor"))
        {
            Debug.LogError("El material proporcionado no tiene emisión o es nulo.");
            return;
        }

        emissionColor = material.GetColor("_EmissionColor");
        material.SetColor("_EmissionColor", emissionColor * minIntensity);

        StartEmissionAnimation();
    }

    private void StartEmissionAnimation()
    {
        Sequence glowSequence = DOTween.Sequence();

        glowSequence
            .Append(material.DOColor(emissionColor * maxIntensity, "_EmissionColor", transitionDuration)
                .SetEase(Ease.InOutCubic))
            .AppendInterval(timeLightDuration)
            .Append(material.DOColor(emissionColor * minIntensity, "_EmissionColor", transitionDuration)
                .SetEase(Ease.InOutCubic))
            .AppendInterval(timeDarkDuration)
            .OnComplete(StartEmissionAnimation);
    }
}