using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configurações do Projétil")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private GameObject hitEffect;

    private Transform target;
    private float damage;

    public void Seek(Transform targetTransform, float projectileDamage)
    {
        target = targetTransform;
        damage = projectileDamage;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.position += direction * distanceThisFrame;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }

    private void HitTarget()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        if (target != null)
        {
            Guard guard = target.GetComponent<Guard>();
            if (guard != null)
            {
                guard.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (target != null && collision.transform == target)
        {
            HitTarget();
        }
    }
}
