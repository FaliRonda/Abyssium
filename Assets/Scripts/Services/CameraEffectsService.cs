using Cinemachine;
using DG.Tweening;
using Ju.Services;
using UnityEngine;

public class CameraEffectsService : IService
{
    public CinemachineVirtualCamera cameraTD;
    
    private bool shakingActive;
    
    public void Initialize(CinemachineVirtualCamera camera)
    {
        cameraTD = camera;
    }
    public void ShakeCamera(float shakeIntensity, float shakeDuration)
    {
        var originalTrackedObjectOffset = cameraTD.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;

        shakingActive = true;

        DOTween.Sequence()
            .Append(DOVirtual.DelayedCall(0, () => ShakeRoutine(shakeIntensity, originalTrackedObjectOffset)))
            .AppendInterval(shakeDuration)
            .AppendCallback(() => { shakingActive = false; })
            .AppendInterval(0.1f)
            .AppendCallback(() => { cameraTD.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = originalTrackedObjectOffset; });
    }
    
    private void ShakeRoutine(float shakeIntensity, Vector3 originalTrackedObjectOffset)
    {
        if (shakingActive)
        {
            DOVirtual.DelayedCall(0.05f, () =>
            {
                Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
                cameraTD.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = originalTrackedObjectOffset + randomOffset;
                ShakeRoutine(shakeIntensity, originalTrackedObjectOffset);
            });
        }
    }
}