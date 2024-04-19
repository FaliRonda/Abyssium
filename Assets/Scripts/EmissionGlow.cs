using System;
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
    private Sequence glowSequence;
    private Color originalColor;

    private void Start()
    {
        InitializeGlowEffect();
    }

    private void OnEnable()
    {
        InitializeGlowEffect();
    }

    private void InitializeGlowEffect()
    {
        if (material == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer = renderer == null ? GetComponentInChildren<Renderer>() : renderer;
            
            foreach (Material currentMaterial in renderer.materials)
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

            originalColor = material.GetColor("_EmissionColor");
            
            if (emissionColor == Color.magenta)
            {
                emissionColor = originalColor;
            }
            material.SetColor("_EmissionColor", emissionColor * minIntensity);

        }
        
        StartEmissionAnimation();
    }

    private void StartEmissionAnimation()
    {
        glowSequence = DOTween.Sequence();

        glowSequence
            .Append(material.DOColor(emissionColor * maxIntensity, "_EmissionColor", transitionDuration)
                .SetEase(Ease.InOutCubic))
            .AppendInterval(timeLightDuration)
            .Append(material.DOColor(emissionColor * minIntensity, "_EmissionColor", transitionDuration)
                .SetEase(Ease.InOutCubic))
            .AppendInterval(timeDarkDuration)
            .OnComplete(() =>
            {
                if (enabled)
                {
                    StartEmissionAnimation();
                }
                else
                {
                    material.SetColor("_EmissionColor", originalColor);
                }
            });
    }

    public void SetEmissionColor(Color runesReactionColor)
    {
        emissionColor = runesReactionColor;
    }

    private void OnDisable()
    {
        emissionColor = originalColor;
    }

    private void OnDestroy()
    {
        if (material != null && originalColor != Color.clear)
        {
            material.SetColor("_EmissionColor", originalColor);
        }
    }
}