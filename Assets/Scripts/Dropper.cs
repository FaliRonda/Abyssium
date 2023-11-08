using UnityEngine;

public class Dropper : MonoBehaviour
{
    public void Drop(GameObject drop)
    {
        var position = transform.position;
        drop.transform.position = new Vector3(position.x, drop.transform.position.y, position.z - 1f);
        GameObject dropInstantiated = Instantiate(drop);
        var interactable = dropInstantiated.GetComponent<Interactable>();
        interactable.SetCanInteract(true);
    }
}
