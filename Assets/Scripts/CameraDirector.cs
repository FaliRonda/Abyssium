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
            bool cameraToOrtographic = false;
            cameraBlending.Priority = 15;
            StartCoroutine(FinishCameraBlending(camera3D, 20, cameraToOrtographic));
        }
        else
        {
            bool cameraToOrtographic = true;
            cinemachineBrain.GetComponent<Camera>().orthographic = true;
            camera3D.Priority = 5;
            StartCoroutine(FinishCameraBlending(cameraBlending, 1, cameraToOrtographic));
        }
    }

    private IEnumerator FinishCameraBlending(CinemachineVirtualCamera virtualCamera, int priority, bool cameraToOrtographic)
    {
        bool perspectiveSwitched = false;

        if (!cameraToOrtographic)
        {
            cinemachineBrain.GetComponent<Camera>().orthographic = cameraToOrtographic;
            perspectiveSwitched = true;
        }
        
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !CamerasTransitionBlending());
        
        virtualCamera.Priority = priority;

        if (!perspectiveSwitched)
        {
            cinemachineBrain.GetComponent<Camera>().orthographic = cameraToOrtographic;
        }
    }

    public bool CamerasTransitionBlending()
    {
        return cinemachineBrain.IsBlending;
    }

    public void Initialize(Transform pjTransform)
    {
        cameraTD.Follow = pjTransform;
        cameraBlending.Follow = pjTransform;
        camera3D.Follow = pjTransform;
    }
}