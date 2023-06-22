using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3D : MonoBehaviour
{
    public void SwitchCamera()
    {
        var camera = gameObject.GetComponent<Camera>();
        camera.enabled = !camera.enabled;
    }
}
