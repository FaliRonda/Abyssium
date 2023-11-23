using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class LateralDialog : MonoBehaviour
{
    public float animationDuration = 0.5f;
    public float dialogLifeTime = 5f;
    
    private RectTransform rectTransform;
    private float initialAnchorPositionX;
    
    void Start()
    {
        rectTransform = GetComponentsInChildren<RectTransform>()[1];

        initialAnchorPositionX = rectTransform.anchoredPosition.x;
        
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
            {
                rectTransform.DOAnchorPos(new Vector2(0, rectTransform.anchoredPosition.y), animationDuration);
            })
            .AppendInterval(dialogLifeTime)
            .AppendCallback(
                () =>
                {
                    rectTransform.DOAnchorPos(new Vector2(initialAnchorPositionX, rectTransform.anchoredPosition.y),
                        animationDuration);
                })
            .AppendInterval(animationDuration)
            .AppendCallback(() => { Destroy(this.gameObject); });
    }
}
