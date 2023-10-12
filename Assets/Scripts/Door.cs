using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator doorAnimator;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        doorAnimator.Play("Door_open");
    }
}