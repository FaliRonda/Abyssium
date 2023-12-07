using UnityEngine;

public class Dropper : MonoBehaviour
{
    public Transform dropSpawn;
    
    public void Drop(GameObject drop)
    {
        if (dropSpawn == null)
        {
            dropSpawn = transform;
        }
        drop.transform.position = dropSpawn.position;
        
        GameObject dropInstantiated = Instantiate(drop);
        var interactable = dropInstantiated.GetComponent<Interactable>();
        interactable.SetCanInteract(true);
    }
}
