using DG.Tweening;
using UnityEngine;

public class Camera2D : MonoBehaviour
{
    public Transform target;
    public float chasingDuration = 2f;

    public void ChaseTarget()
    {
        Vector3 position = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.DOMove(position, chasingDuration)
            .SetEase(Ease.InOutSine);
    }

    public void SwitchCamera()
    {
        var camera = gameObject.GetComponent<Camera>();
        camera.enabled = !camera.enabled;
    }

    public bool IsEnabled()
    {
        var camera = gameObject.GetComponent<Camera>();
        return camera.enabled;
    }
}
