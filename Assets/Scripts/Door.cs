using System;
using DG.Tweening;
using Ju.Extensions;
using UnityEngine;

public class Door : MonoBehaviour
{
    public RoomDissolve room;
    public bool doorOpening;
    
    private Animator doorAnimator;
    private BoxCollider collider;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
        collider = GetComponentInChildren<BoxCollider>();
    }

    public void OpenDoor()
    {
        if (!doorOpening)
        {
            doorOpening = true;
            doorAnimator.Play("Door_open");
            float animLenght = Core.AnimatorHelper.GetAnimLenght(doorAnimator, "Door_open");
            Core.AnimatorHelper.DoOnAnimationFinish(animLenght, OpenAndDissolveRoom);
        }
    }

    void OpenAndDissolveRoom(string t)
    {
        collider.enabled = false;
        room.Disolve();
    }

    public void CloseDoor()
    {
        doorAnimator.Play("Door_close");
        collider.enabled = true;
    }
}