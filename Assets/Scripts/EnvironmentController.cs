using System.Collections.Generic;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnvironmentController : MonoBehaviour
{
    public TilemapRenderer roof;
    public List<GameObject> decorationElements;

    public void Start()
    {
        this.EventSubscribe<SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    
    public void Switch2D3D(bool gameIn3D)
    {
        roof.sortingLayerName = gameIn3D ? "Background" : "Player";

        foreach (GameObject decorationElement in decorationElements)
        {
            if (gameIn3D)
            {
                decorationElement.GetComponentInChildren<SpriteRenderer>().transform.Rotate(new Vector3(-45, 0, 0));
            }
            else
            {
                decorationElement.GetComponentInChildren<SpriteRenderer>().transform.Rotate(new Vector3(45, 0, 0));
            }
        }
    }
}