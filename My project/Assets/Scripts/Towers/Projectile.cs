using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configurações do Projétil")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float rotationOffset = 0f; // Ajuste de rotação para o sprite da bala
    [SerializeField] private GameObject impactEffect;

    private Transform target;
    private float damage;

    public void Seek(Transform targetEnemy, float projectileDamage)
    {
        target = targetEnemy;
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

        // Se estiver perto o suficiente para colidir neste frame
        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.position += direction * distanceThisFrame;

        // Rotaciona o projétil na direção do movimento (2D)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }

    private void HitTarget()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        if (target != null)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
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
