using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Services;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraEffectsService : IService
{
    public CinemachineVirtualCamera cameraTD;
    public CinemachineVirtualCamera camera3D;
    public CinemachineTargetGroup cameraTargetGroup;
    
    private bool shakingActive;
    private float originalScreenX;
    private float originalScreenY;
    private CinemachineVirtualCamera currentCamera;
    private TweenerCore<Vector3, Vector3, VectorOptions> shakeMovement;
    private Sequence shakeMovementSequence;
    private GameObject tempCenterGO;
    private Transform pjTransform;
    private Sequence shakeDurationCDSequence;

    public void Initialize(CinemachineVirtualCamera cameraTD, CinemachineVirtualCamera camera3D, CinemachineTargetGroup cameraTargetGroup)
    {
        this.cameraTD = cameraTD;
        this.camera3D = camera3D;
        this.cameraTargetGroup = cameraTargetGroup;
        originalScreenX = 0.5f;
        originalScreenY = 0.5f;
        shakeDurationCDSequence = DOTween.Sequence();
        shakeMovementSequence = DOTween.Sequence();
    }
    
    public void StartShakingEffect(float shakeIntensity, float shakeFrequency, float shakeDuration)
    {
        shakeDurationCDSequence.Kill();
        shakingActive = true;
        currentCamera = GameState.gameIn3D ? camera3D : cameraTD;

        shakeDurationCDSequence = DOTween.Sequence();
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
                x => currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX =
                    originalScreenX + x, randomValue.x * shakeIntensity, shakeFrequency)
            .SetEase(Ease.OutBack))
            .Join(DOTween.To(() => 0,
                    x => currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY =
                        originalScreenY + x, randomValue.y * shakeIntensity, shakeFrequency)
                .SetEase(Ease.OutBack))
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
                x => currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>()
                    .m_ScreenX = x,
                originalScreenX,
                shakeDuration)
            .SetEase(Ease.OutBack);

        DOTween.To(() => originalScreenY,
                x => currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>()
                    .m_ScreenY = x,
                originalScreenY,
                shakeDuration)
            .SetEase(Ease.OutBack);
    }

    public void ZoomOut(int zoomDuration)
    {
        DOTween.To(() => cameraTD.m_Lens.OrthographicSize, x => cameraTD.m_Lens.OrthographicSize = x, 10, zoomDuration);
    }
    
    public void ZoomIn(int zoomDuration)
    {
        DOTween.To(() => cameraTD.m_Lens.OrthographicSize, x => cameraTD.m_Lens.OrthographicSize = x, 5f, zoomDuration);
    }

    public void SetPJVisibility(bool showPJ)
    {
        if (showPJ)
        {
            Camera.main.cullingMask = -1;
        }
        else
        {
            int cullingMask = Camera.main.cullingMask;
            cullingMask &= ~(1 << Layers.PJ_LAYER);
            Camera.main.cullingMask = cullingMask;
        }
    }

    public void ToCenter()
    {
        currentCamera = GameState.gameIn3D ? camera3D : cameraTD;
        pjTransform = currentCamera.Follow;

        tempCenterGO = GameObject.Instantiate(new GameObject(), pjTransform.parent.parent);
        tempCenterGO.transform.position = new Vector3(0, 0, 2.5f);

        cameraTargetGroup.AddMember(tempCenterGO.transform, 5, 3f);
    }

    public void ToPlayer()
    {
        currentCamera = GameState.gameIn3D ? camera3D : cameraTD;
        cameraTargetGroup.RemoveMember(cameraTargetGroup.m_Targets[cameraTargetGroup.m_Targets.Length - 2].target);
        GameObject.Destroy(tempCenterGO);
    }

    public void AddTransformToTargetGroup(Transform instantiatedEnemy, float weight, float radius)
    {
        cameraTargetGroup.AddMember(instantiatedEnemy, weight, radius);
    }

    public void RemoveTransformFromTargetGroup(Transform defeatedEnemy)
    {
        cameraTargetGroup.RemoveMember(defeatedEnemy);
    }
}