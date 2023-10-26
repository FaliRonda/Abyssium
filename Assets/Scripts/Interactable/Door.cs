using System;
using DG.Tweening;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class Door : Interactable
{
    public RoomDissolve room;
    public bool doorOpening;
    
    
    [System.Serializable]
    public class DoorOpenedCallbackEvent : UnityEvent<bool> {}
    public DoorOpenedCallbackEvent doorOpenedCallback;
    
    
    private Animator doorAnimator;
    private BoxCollider doorBoxCollider;

    private void Start()
    {
        doorAnimator = GetComponentInParent<Animator>();
        var colliders = GetComponentsInParent<BoxCollider>();
        doorBoxCollider = colliders[1];
    }

    public override void Interact(PJ pj)
    {
        if (CanInteract())
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (!doorOpening)
        {
            doorOpening = true;
            doorAnimator.Play("Door_open");
            float animLenght = Core.AnimatorHelper.GetAnimLenght(doorAnimator, "Door_open");
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
        Core.Event.Fire(new GameEvents.LoadFloorSceneEvent() {isFloorBelow = isFloorBelow});
    }

    public void CloseDoor()
    {
        doorAnimator.Play("Door_close");
        doorBoxCollider.enabled = true;
    }
}