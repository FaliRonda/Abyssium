using System;
using System.Collections;
using System.Collections.Generic;
using Ju.Extensions;
using Unity.VisualScripting;
using UnityEngine;

public class TranslateIn3D : MonoBehaviour
{
    public Vector3 translation;
    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;

    }
    
    public void Start()
    {
        this.EventSubscribe<SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    public void Switch2D3D(bool gameIn3D)
    {
        if (gameIn3D)
        {
            transform.position += translation;
        }
        else
        {
            transform.position = initialPosition;
        }
    }
}
