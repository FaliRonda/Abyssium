using Ju.Extensions;
using UnityEngine;

public class EnableOn3D : MonoBehaviour
{
    public bool disableOn3D = false;
    
    public void Start()
    {
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
        SwitchActiveChilds(false);
    }

    private void Switch2D3D(bool gameIn3D)
    {
        SwitchActiveChilds(gameIn3D);
    }

    private void SwitchActiveChilds(bool active)
    {
        foreach (Transform child in transform)
        {
            if (!disableOn3D)
            {
                child.gameObject.SetActive(active);
            }
            else
            {
                child.gameObject.SetActive(!active);
            }
        }
    }
}
