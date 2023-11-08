using Ju.Extensions;
using UnityEngine;

public class UpdateSortingLayerOn3D : MonoBehaviour
{
    public int sortingOrderIn3D;

    private int originalSortingOrder;
    private SpriteRenderer sprite;
    
    public void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalSortingOrder = sprite.sortingOrder;
        
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    private void Switch2D3D(bool gameIn3D)
    {
        if (gameIn3D)
        {
            sprite.sortingOrder = sortingOrderIn3D;
        }
        else
        {
            sprite.sortingOrder = originalSortingOrder;
        }
    }
}
