using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyGroup
    {
        public string groupName = "Grupo de Inimigos";
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 1f;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Onda 1";
        public EnemyGroup[] enemyGroups;
        public float timeAfterWave = 5f;
    }

    [Header("Configuração de Ondas")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float initialDelay = 2f;

    [Header("Ponto de Spawn")]
    [SerializeField] private Transform spawnPoint;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaves => waves != null ? waves.Length : 0;

    private void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        StartCoroutine(SpawnWavesRoutine());
    }

    private IEnumerator SpawnWavesRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];
            Debug.Log($"=== Iniciando {wave.waveName} ({currentWaveIndex + 1}/{waves.Length}) ===");

            if (wave.enemyGroups != null)
            {
                foreach (EnemyGroup group in wave.enemyGroups)
                {
                    if (group.enemyPrefab == null) continue;

                    for (int i = 0; i < group.count; i++)
                    {
                        SpawnEnemy(group.enemyPrefab);
                        yield return new WaitForSeconds(group.spawnInterval);
                    }
                }
            }

            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                yield return new WaitForSeconds(wave.timeAfterWave);
            }
        }

        Debug.Log("Todas as ondas de inimigos foram concluídas!");
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
