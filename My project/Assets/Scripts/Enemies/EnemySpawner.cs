using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public enum PathSelectionMode
    {
        AlternatePaths,  // Alterna entre Caminho 1 e Caminho 2 a cada inimigo
        RandomPath,      // Escolhe um caminho aleatório para cada inimigo
        AllPathsSimultaneously // Spawna em todos os caminhos ao mesmo tempo
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string groupName = "Grupo de Inimigos";
        public GameObject enemyPrefab;
        public int count = 6;
        public float spawnInterval = 1.2f;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Onda 1";
        public EnemyGroup[] enemyGroups;
        public float timeAfterWave = 5f;
    }

    [Header("Caminhos / Rotas")]
    [Tooltip("Arraste os objetos de Caminho aqui (ex: CaminhoSuperior, CaminhoInferior). Se deixar vazio, ele encontra automaticamente na cena.")]
    [SerializeField] private WaypointPath[] paths;
    [SerializeField] private PathSelectionMode pathMode = PathSelectionMode.AlternatePaths;

    [Header("Configuração de Ondas")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float initialDelay = 2f;

    private int currentWaveIndex = 0;
    private int currentPathIndex = 0;

    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaves => waves != null ? waves.Length : 0;

    private void Start()
    {
        // Se nenhum caminho foi atribuído manualmente, busca todos na cena
        if (paths == null || paths.Length == 0)
        {
            paths = FindObjectsOfType<WaypointPath>();
            if (paths.Length == 0 && WaypointPath.AllPaths.Count > 0)
            {
                paths = WaypointPath.AllPaths.ToArray();
            }
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
                        SpawnEnemyOnPath(group.enemyPrefab);
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

    private void SpawnEnemyOnPath(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return;

        // Atualiza a lista de caminhos se estiver vazia
        if (paths == null || paths.Length == 0)
        {
            paths = FindObjectsOfType<WaypointPath>();
            if (paths.Length == 0)
            {
                Debug.LogWarning("[EnemySpawner] Nenhum WaypointPath encontrado na cena!");
                Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                return;
            }
        }

        if (pathMode == PathSelectionMode.AllPathsSimultaneously)
        {
            foreach (WaypointPath path in paths)
            {
                if (path != null) SpawnSingleEnemy(enemyPrefab, path);
            }
        }
        else
        {
            WaypointPath chosenPath = GetNextPath();
            if (chosenPath != null)
            {
                SpawnSingleEnemy(enemyPrefab, chosenPath);
            }
        }
    }

    private WaypointPath GetNextPath()
    {
        if (paths == null || paths.Length == 0) return null;

        if (pathMode == PathSelectionMode.RandomPath)
        {
            return paths[Random.Range(0, paths.Length)];
        }
        else // AlternatePaths
        {
            WaypointPath path = paths[currentPathIndex % paths.Length];
            currentPathIndex++;
            return path;
        }
    }

    private void SpawnSingleEnemy(GameObject enemyPrefab, WaypointPath path)
    {
        Vector3 spawnPos = path.GetStartPoint();
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.SetPath(path.GetWaypoints());
        }
    }
}
