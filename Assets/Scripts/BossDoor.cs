using DG.Tweening;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    public GameObject triggersGO;
    private void Start()
    {
        Sequence colliderAppearSequence = DOTween.Sequence();

        colliderAppearSequence
            .AppendInterval(1f)
            .AppendCallback(() => { triggersGO.SetActive(true); });
    }

    public void Appear()
    {
        foreach (Transform child in transform)
        {
            if (!child.gameObject.name.Contains("Trigger"))
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    
    public void Disappear()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name.Contains("FX"))
            {
                Sequence fxSequence = DOTween.Sequence();
                ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
                fxSequence
                    .AppendCallback(() => { particleSystem.Play(); })
                    .AppendInterval(particleSystem.main.startLifetime.constant)
                    .AppendCallback(() => { child.gameObject.SetActive(false); });
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            
        }
    }
}
