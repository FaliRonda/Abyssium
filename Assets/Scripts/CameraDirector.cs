using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraDirector : MonoBehaviour
{
    public CinemachineVirtualCamera camera2D;
    public CinemachineVirtualCamera camera3D;

    public LayerMask cullingMask3D;
    private LayerMask _savedLayerMask;
    
    private CinemachineBrain cinemachineBrain;

    private void Start()
    {
        cinemachineBrain = gameObject.GetComponent<CinemachineBrain>();
        _savedLayerMask = Camera.main.cullingMask;
    }

    public void Switch2D3D(bool gameIn3D)
    {
        if (gameIn3D)
        {
            camera3D.Priority = 15;
            Camera.main.cullingMask = cullingMask3D;
        }
        else
        {
            camera3D.Priority = 5;
            Camera.main.cullingMask = _savedLayerMask;
        }
    }
}
