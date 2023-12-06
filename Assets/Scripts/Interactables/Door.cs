using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Door : Interactable
{
    public RoomDissolve room;
    public bool doorOpening;
    public bool isLocked = false;

    public Material orbsOutlineDoor;
    public Material blackOrbDoor;
    public Material whiteOrbDoor;
    public Material twoOrbsDoor;
    
    
    [System.Serializable]
    public class DoorOpenedCallbackEvent : UnityEvent<bool> {}
    public DoorOpenedCallbackEvent doorOpenedCallback;

    private SkinnedMeshRenderer mesh;
    private Animator doorAnimator;
    private BoxCollider doorBoxCollider;

    private void Start()
    {
        mesh = GetComponentInParent<SkinnedMeshRenderer>();
        doorAnimator = GetComponentInParent<Animator>();
        var colliders = GetComponentsInParent<BoxCollider>();
        doorBoxCollider = colliders[1];
    }

    public override void Interact(PJ pj)
    {
        if (CanInteract())
        {
            TryOpenDoor(pj);
        }
    }

    public void TryOpenDoor(PJ pj)
    {
        if (!isLocked)
        {
            OpenDoor();
        }
        else
        {
            Renderer doorRenderer = GetComponent<Renderer>();
            Material[] newMaterials = doorRenderer.materials;
            
            if (pj.inventory.HasBlackOrb)
            {
                newMaterials[1] = blackOrbDoor;
            }

            if (pj.inventory.HasWhiteOrb)
            {
                newMaterials[1] = whiteOrbDoor;
            }
            
            if (pj.inventory.HasBlackOrb && pj.inventory.HasWhiteOrb)
            {
                newMaterials[1] = twoOrbsDoor;
                isLocked = false;
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.3f)
                    .AppendCallback(() => OpenDoor())
                    .AppendCallback(() => Core.Event.Fire(new GameEvents.DoorOpened()));
            }

            doorRenderer.materials = newMaterials;
        }
    }

    private void OpenDoor()
    {
        if (!doorOpening)
        {
            doorOpening = true;
            doorAnimator.Play("Door_open");
            float animLenght = Core.AnimatorHelper.GetAnimLength(doorAnimator, "Door_open");
            Core.AnimatorHelper.DoOnAnimationFinish(animLenght, CallbackOnDoorOpened);
        }
    }

    public void CallbackOnDoorOpened()
    {
        doorOpenedCallback.Invoke(false);
    }

    public void OpenAndDissolveRoom()
    {   
        doorBoxCollider.enabled = false;
        SetCanInteract(false);
        if (room != null)
        {
            room.Disolve();
        }
    }

    public void LoadNewFloor(bool isFloorBelow)
    {
        Core.Event.Fire(new GameEvents.LoadFloorSceneEvent() {toFloorBelow = isFloorBelow});
    }

    public void CloseDoor()
    {
        doorAnimator.Play("Door_close");
        doorBoxCollider.enabled = true;
    }
}