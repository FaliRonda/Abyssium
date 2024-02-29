using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Services;
using UnityEngine;

public class CameraEffectsService : IService
{
    public CinemachineVirtualCamera cameraTD;
    public CinemachineVirtualCamera camera3D;
    
    private bool shakingActive;
    private float originalScreenX;
    private float originalScreenY;
    private CinemachineVirtualCamera cameraToShake;
    private TweenerCore<Vector3, Vector3, VectorOptions> shakeMovement;
    private Sequence shakeMovementSequence;

    public void Initialize(CinemachineVirtualCamera cameraTD, CinemachineVirtualCamera camera3D)
    {
        this.cameraTD = cameraTD;
        this.camera3D = camera3D;
    }
    public void StartShakingEffect(float shakeIntensity, float shakeFrequency, float shakeDuration)
    {
        shakingActive = true;
        cameraToShake = GameState.gameIn3D ? camera3D : cameraTD;
        originalScreenX = cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX;
        originalScreenY = cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY;

        Sequence shakeDurationCDSequence = DOTween.Sequence();
        shakeDurationCDSequence
            .AppendInterval(shakeDuration)
            .AppendCallback(() =>
            {
                shakingActive = false;
                shakeMovementSequence.Kill();
                ResetCameraOffset(shakeFrequency);
            });
        
        DoShake(shakeIntensity, shakeFrequency);
    }
    
    private void DoShake(float shakeIntensity, float shakeFrequency)
    {
        Vector2 randomValue = Random.insideUnitCircle / 2f;

        shakeMovementSequence = DOTween.Sequence();
        
        shakeMovementSequence
            .Append(DOTween.To(() => 0,
                x => cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX =
                    originalScreenX + x, randomValue.x * shakeIntensity, shakeFrequency)
            .SetEase(Ease.InOutQuad))
            .Join(DOTween.To(() => 0,
                    x => cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY =
                        originalScreenY + x, randomValue.y * shakeIntensity, shakeFrequency)
                .SetEase(Ease.InOutQuad))
            .OnComplete(() =>
            {
                // Si hay una duración de efecto especificada, iniciamos el siguiente movimiento de sacudida.
                if (shakingActive)
                {
                    DoShake(shakeIntensity, shakeFrequency);
                }
            });
    }
    
    private void ResetCameraOffset(float shakeDuration)
    {
        DOTween.To(() => originalScreenX,
                x => cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>()
                    .m_ScreenX = x,
                originalScreenX,
                shakeDuration)
            .SetEase(Ease.InOutQuad);

        DOTween.To(() => originalScreenY,
                x => cameraToShake.GetCinemachineComponent<CinemachineFramingTransposer>()
                    .m_ScreenY = x,
                originalScreenY,
                shakeDuration)
            .SetEase(Ease.InOutQuad);
    }

    public void ZoomOut(int zoomDuration)
    {
        DOTween.To(() => cameraTD.m_Lens.OrthographicSize, x => cameraTD.m_Lens.OrthographicSize = x, 10, zoomDuration);
    }
    
    public void ZoomIn(int zoomDuration)
    {
        DOTween.To(() => cameraTD.m_Lens.OrthographicSize, x => cameraTD.m_Lens.OrthographicSize = x, 5f, zoomDuration);
    }
}