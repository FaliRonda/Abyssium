using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 2f;
    private Transform playerTransform;

    public void Initialize(float bulletSpeed, float bulletLifeTime, Transform playerTransform)
    {
        speed = bulletSpeed;
        lifeTime = bulletLifeTime;
        this.playerTransform = playerTransform;
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
        // Puedes agregar aquí cualquier lógica adicional antes de destruir el objeto
        Destroy(gameObject);
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.PJ_LAYER)
        {
            other.GetComponent<PJ>().GetDamage();
            Destroy(gameObject);
        }
    }
}
