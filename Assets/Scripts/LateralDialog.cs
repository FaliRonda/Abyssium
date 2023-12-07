using DG.Tweening;
using UnityEngine;

public class LateralDialog : MonoBehaviour
{
    public float animationDuration = 0.5f;
    public float dialogLifeTime = 10f;
    
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
                        animationDuration / 2f);
                })
            .AppendInterval(animationDuration / 2f)
            .AppendCallback(() => { Destroy(this.gameObject); });
    }
}
