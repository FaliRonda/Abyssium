using System;
using System.Collections;
using System.Collections.Generic;
using Ju.Extensions;
using Unity.VisualScripting;
using UnityEngine;

public class RotateIn3D : MonoBehaviour
{
    private Quaternion initialRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;

    }
    
    public void Start()
    {
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    private void Switch2D3D(bool gameIn3D)
    {
        if (gameIn3D)
        {
            transform.Rotate(new Vector3(-45, 0, 0));
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }
}
