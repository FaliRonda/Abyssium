using Cinemachine;
using DG.Tweening;
using Ju.Services;
using UnityEngine;

public class CameraEffectsService : IService
{
    public CinemachineVirtualCamera cameraTD;
    public CinemachineVirtualCamera camera3D;
    
    private bool shakingActive;
    
    public void Initialize(CinemachineVirtualCamera cameraTD, CinemachineVirtualCamera camera3D)
    {
        this.cameraTD = cameraTD;
        this.camera3D = camera3D;
    }
    public void ShakeCamera(float shakeIntensity, float shakeDuration)
    {
        var originalTrackedObjectOffset = cameraTD.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;

        shakingActive = true;

        CinemachineVirtualCamera cameraToShake = GameState.gameIn3D ? camera3D : cameraTD;

        DOTween.Sequence()
            .Append(DOVirtual.DelayedCall(0, () => ShakeRoutine(cameraToShake, shakeIntensity, originalTrackedObjectOffset)))
            .AppendInterval(shakeDuration)
            .AppendCallback(() => { shakingActive = false; })
            .AppendInterval(0.1f)
            .AppendCallback(() => { cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = originalTrackedObjectOffset; });
    }
    
    private void ShakeRoutine(CinemachineVirtualCamera cameraToShake, float shakeIntensity, Vector3 originalTrackedObjectOffset)
    {
        if (shakingActive)
        {
            DOVirtual.DelayedCall(0.05f, () =>
            {
                Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
                cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset = originalTrackedObjectOffset + randomOffset;
                ShakeRoutine(cameraToShake, shakeIntensity, originalTrackedObjectOffset);
            });
        }
    }
}