using System;
using DG.Tweening;
using Ju.Services;
using Bloom = UnityEngine.Rendering.Universal.Bloom;
using ChromaticAberration = UnityEngine.Rendering.Universal.ChromaticAberration;
using Vignette = UnityEngine.Rendering.Universal.Vignette;
using UnityEngine.Rendering;
using UnityEngine.Windows.Speech;

public class PostProcessingService : IService
{
    private Vignette vignette;
    private float originalVignetteIntensity;
    private float originalVignetteSmoothness;
    
    private Bloom bloom;
    
    private ChromaticAberration chromaticAberration;
    

    public enum EFFECTS
    {
        VIGNETTE,
        BLOOM,
        CHROMATIC_ABERRATION
    }

    public void Initialize(Volume postprocessing)
    {
        postprocessing.profile.TryGet(out vignette);
        postprocessing.profile.TryGet(out bloom);
        postprocessing.profile.TryGet(out chromaticAberration);

        originalVignetteIntensity = vignette.intensity.value;
        originalVignetteSmoothness = vignette.smoothness.value;
    }

    public void SetEffectValue(EFFECTS effect, float value)
    {
        switch (effect)
        {
            case EFFECTS.VIGNETTE:
                SetVolumeParameterValues(vignette.intensity, vignette.smoothness, value);
                break;
            case EFFECTS.BLOOM:
                SetVolumeParameterValues(bloom.intensity, null, value);
                break;
            case EFFECTS.CHROMATIC_ABERRATION:
                SetVolumeParameterValues(chromaticAberration.intensity, null, value);
                break;
            default:
                break;
        }
    }

    private void SetVolumeParameterValues(VolumeParameter<float> intensity, VolumeParameter<float> smoothness, float value)
    {
        if (intensity != null)
        {
            intensity.value = value;
        }
        if (smoothness != null)
        {
            smoothness.value = value;
        }
    }

    public Sequence PlayBlinkEffectAnimation(EFFECTS effect, float endValue, float inTransitionDuration, float outTransitionDuration,
        float waitDuration)
    {
        Sequence effectPingPongSequence = DOTween.Sequence();
        
        switch (effect)
        {
            case EFFECTS.VIGNETTE:
                effectPingPongSequence = CreateEffectSequence(vignette.intensity, vignette.smoothness, endValue, inTransitionDuration, outTransitionDuration, waitDuration, originalVignetteIntensity, originalVignetteSmoothness);
                break;
            case EFFECTS.BLOOM:
                effectPingPongSequence = CreateEffectSequence(bloom.intensity, null, endValue, inTransitionDuration, outTransitionDuration, waitDuration, 0.05f, 2f);
                break;
            case EFFECTS.CHROMATIC_ABERRATION:
                effectPingPongSequence = CreateEffectSequence(chromaticAberration.intensity, null, endValue, inTransitionDuration, outTransitionDuration, waitDuration, 0.2f, 1f);
                break;
            default:
                break;
        }

        return effectPingPongSequence;
    }
    
    private Sequence CreateEffectSequence(VolumeParameter<float> intensity, VolumeParameter<float> smoothness, float endValue, float inTransitionDuration, float outTransitionDuration, float waitDuration, float originalIntensity, float originalSmoothness)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(DOTween.To(() => intensity.value, x => intensity.value = x, endValue, inTransitionDuration).SetEase(Ease.OutQuad));
        if (smoothness != null)
        {
            sequence.Join(DOTween.To(() => smoothness.value, x => smoothness.value = x, endValue, inTransitionDuration).SetEase(Ease.OutQuad));
        }
        sequence.AppendInterval(waitDuration);
        sequence.Append(DOTween.To(() => intensity.value, x => intensity.value = x, originalIntensity, outTransitionDuration).SetEase(Ease.OutQuad));
        if (smoothness != null)
        {
            sequence.Join(DOTween.To(() => smoothness.value, x => smoothness.value = x, originalSmoothness, outTransitionDuration).SetEase(Ease.OutQuad));
        }

        return sequence;
    }

    public Sequence ConceptProtoStartVignetteBlinkingEffect()
    {
        Sequence blinkingSequence = DOTween.Sequence();
        
        blinkingSequence
            .AppendInterval(2f)
            
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))

            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))

            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.75f, 0.3f)
                .SetEase(Ease.OutQuad))

            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))
            .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 1f, 0.3f)
                .SetEase(Ease.OutQuad))

            .AppendInterval(1f)

            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.55f, 1f)
                .SetEase(Ease.OutQuad))
            .Join(DOTween.To(() => vignette.smoothness.value, x => vignette.smoothness.value = x, 0.55f, 1f)
                .SetEase(Ease.OutQuad));
        
        return blinkingSequence;
    }

    public Sequence CameraSwitchBlinkAnimation(Action betweenBlinksCallback)
    {
        Sequence cameraSwitchBlinkSequence = DOTween.Sequence();
        cameraSwitchBlinkSequence
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.1f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.75f, 1f)
                .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 0.1f)
                .SetEase(Ease.OutQuad))
            .AppendCallback(() => betweenBlinksCallback.Invoke())
            .Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 0.3f)
                .SetEase(Ease.OutQuad))
            .AppendCallback(() => { GameState.controlBlocked = false; });

        return cameraSwitchBlinkSequence;
    }
}