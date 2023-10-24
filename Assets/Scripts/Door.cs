using System;
using DG.Tweening;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public RoomDissolve room;
    public bool doorOpening;
    
    
    [System.Serializable]
    public class DoorOpenedCallbackEvent : UnityEvent<bool> {}
    public DoorOpenedCallbackEvent doorOpenedCallback;
    
    
    private Animator doorAnimator;
    private BoxCollider boxCollider;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
        boxCollider = GetComponentInChildren<BoxCollider>();
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
        boxCollider.enabled = false;
        room.Disolve();
    }

    public void LoadNewFloor(bool isFloorBelow)
    {
        Core.Event.Fire(new GameEvents.LoadFloorSceneEvent() {isFloorBelow = isFloorBelow});
    }

    public void CloseDoor()
    {
        doorAnimator.Play("Door_close");
        boxCollider.enabled = true;
    }
}