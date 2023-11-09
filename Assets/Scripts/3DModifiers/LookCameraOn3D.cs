using Ju.Extensions;
using UnityEngine;

public class LookCameraOn3D : MonoBehaviour
{
    private bool gameIn3D;
    private Quaternion defaultSpriteRotation;
    public bool rotateCameraOn3DActive = true;
    
    public void Start()
    {
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>((e) =>  this.gameIn3D = e.gameIn3D);
        defaultSpriteRotation = transform.rotation;
    }

    private void Update()
    {
        if (rotateCameraOn3DActive)
        {
            if (gameIn3D)
            {
                Quaternion lookRotation = Camera.main.transform.rotation;
                transform.rotation = lookRotation;
            }
            else
            {
                transform.rotation = defaultSpriteRotation;
            }
        }
    }
}
