using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [Header("Destruição Automática")]
    [SerializeField] private float lifetime = 1.5f;

    private void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
        }

        Destroy(gameObject, lifetime);
    }
}
