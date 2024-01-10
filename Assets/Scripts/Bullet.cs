using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 2f;

    public void Initialize(float bulletSpeed, float bulletLifeTime)
    {
        speed = bulletSpeed;
        lifeTime = bulletLifeTime;
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x, 0.3f, position.z);
    }
    
    public void StartShoot(Vector3 direction)
    {
        // Calcular la duración de la animación en función de la velocidad y la distancia
        float distance = speed * lifeTime;
        // Mover el objeto utilizando DOTween con una duración constante
        transform.DOMove(transform.position + direction.normalized * distance, lifeTime)
            .SetEase(Ease.Linear)
            .OnComplete(DestroyBullet);
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
    
    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == Layers.PJ_LAYER)
        {
            if (other.GetComponent<PJ>().GetDamage(transform))
            {
                DestroyBullet();
            }
        }
        else if (other.gameObject.layer == Layers.WEAPON_LAYER)
        {
            DestroyBullet();
        }
    }
}
