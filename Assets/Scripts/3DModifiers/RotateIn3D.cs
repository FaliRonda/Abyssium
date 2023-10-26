using Ju.Extensions;
using UnityEngine;

public class RotateIn3D : MonoBehaviour
{
    public bool vertical = true;
    
    private Quaternion initialRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }
    
    public void Start()
    {
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    private void Switch2D3D(bool gameIn3D)
    {
        if (gameIn3D)
        {
            var xRotation = vertical ? -45 : 45;
            transform.Rotate(new Vector3(xRotation, 0, 0));
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }
}
