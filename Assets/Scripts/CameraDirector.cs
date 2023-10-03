using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraDirector : MonoBehaviour
{
    public CinemachineVirtualCamera cameraTD;
    public CinemachineVirtualCamera cameraBlending;
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
            cameraBlending.Priority = 15;
            StartCoroutine(FinishCameraBlending(camera3D, 20));
        }
        else
        {
            camera3D.Priority = 5;
            StartCoroutine(FinishCameraBlending(cameraBlending, 1));
        }
    }

    private IEnumerator FinishCameraBlending(CinemachineVirtualCamera virtualCamera, int priority)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !CamerasTransitionBlending());
        virtualCamera.Priority = priority;
    }

    public bool CamerasTransitionBlending()
    {
        return cinemachineBrain.IsBlending;
    }
}