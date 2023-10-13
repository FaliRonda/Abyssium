using System;
using System.Collections;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class RoomDissolve : MonoBehaviour
{
    public float dissolveSpeed = 1f;
    private Material material;
    
    private float dissolveDelta;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
        material.SetFloat("_DissolveAmount", 2f);
    }

    [Button]
    public void Disolve()
    {
        StartCoroutine(DisolveRoutine());
    }

    private IEnumerator DisolveRoutine()
    {
        float initialDissolveAmount = material.GetFloat("_DissolveAmount");
        while (material.GetFloat("_DissolveAmount") >= 0)
        {
            dissolveDelta += dissolveSpeed * Time.deltaTime;
            material.SetFloat("_DissolveAmount", initialDissolveAmount - dissolveDelta);
            yield return null;
        }

        dissolveDelta = 0;
        yield return null;
    }
}
