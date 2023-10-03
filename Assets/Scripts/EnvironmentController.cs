using UnityEngine;
using UnityEngine.Tilemaps;

public class EnvironmentController : MonoBehaviour
{
    public TilemapRenderer roof;

    public void Switch2D3D(bool gameIn3D)
    {
        roof.sortingLayerName = gameIn3D ? "Background" : "Player";
    }
}