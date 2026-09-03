using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

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
        public int count = 4;
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
    [Tooltip("Arraste os objetos de Caminho aqui (ex: Caminho, CaminhoDcima). Se deixar vazio ou com elementos vazios, ele encontra automaticamente todos na cena!")]
    [SerializeField] private List<WaypointPath> paths = new List<WaypointPath>();
    [SerializeField] private PathSelectionMode pathMode = PathSelectionMode.AlternatePaths;

    [Header("Configuração de Ondas")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private float initialDelay = 2f;

    public static event Action OnAllWavesCompleted;
    public static event Action<int, int> OnWaveChanged; // (currentWave, totalWaves)

    private int currentWaveIndex = 0;
    private int currentPathIndex = 0;
    private bool allWavesCompleted = false;

    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaves => waves != null ? waves.Length : 0;
    public bool AreAllWavesCompleted => allWavesCompleted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RefreshAndSanitizePaths();
    }

    private void Start()
    {
        RefreshAndSanitizePaths();
        StartCoroutine(SpawnWavesRoutine());
    }

    /// <summary>
    /// Limpa elementos nulos da lista de caminhos e busca os caminhos da cena se estiver vazia
    /// </summary>
    public void RefreshAndSanitizePaths()
    {
        // 1. Remove qualquer elemento nulo (None) que tenha ficado no Inspector
        paths.RemoveAll(p => p == null);

        // 2. Se a lista estiver vazia após a limpeza, busca todos os caminhos presentes na cena
        if (paths.Count == 0)
        {
            WaypointPath[] foundInScene = FindObjectsOfType<WaypointPath>();
            foreach (WaypointPath wp in foundInScene)
            {
                if (wp != null && !paths.Contains(wp))
                {
                    paths.Add(wp);
                }
            }

            if (paths.Count == 0 && WaypointPath.AllPaths.Count > 0)
            {
                foreach (WaypointPath wp in WaypointPath.AllPaths)
                {
                    if (wp != null && !paths.Contains(wp))
                    {
                        paths.Add(wp);
                    }
                }
            }
        }

        Debug.Log($"[EnemySpawner] Total de caminhos ativos configurados: {paths.Count}");
        foreach (var p in paths)
        {
            Debug.Log($"[EnemySpawner] -> Caminho ativo: {p.gameObject.name} ({p.GetWaypoints().Length} waypoints)");
        }
    }

    private IEnumerator SpawnWavesRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] Nenhuma onda foi configurada no EnemySpawner!");
            allWavesCompleted = true;
            OnAllWavesCompleted?.Invoke();
            yield break;
        }

        while (currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];
            OnWaveChanged?.Invoke(currentWaveIndex + 1, waves.Length);
            Debug.Log($"=== Iniciando {wave.waveName} ({currentWaveIndex + 1}/{waves.Length}) ===");

            if (wave.enemyGroups != null)
            {
                foreach (EnemyGroup group in wave.enemyGroups)
                {
                    if (group == null) continue;

                    if (group.enemyPrefab == null)
                    {
                        Debug.LogWarning($"[EnemySpawner] O grupo '{group.groupName}' na {wave.waveName} está sem Prefab de Inimigo! Pulando...");
                        continue;
                    }

                    for (int i = 0; i < group.count; i++)
                    {
                        SpawnEnemyOnPath(group.enemyPrefab);
                        yield return new WaitForSeconds(Mathf.Max(0.1f, group.spawnInterval));
                    }
                }
            }

            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                yield return new WaitForSeconds(Mathf.Max(0.5f, wave.timeAfterWave));
            }
        }

        allWavesCompleted = true;
        OnAllWavesCompleted?.Invoke();
        Debug.Log("🎉 [EnemySpawner] Todas as ondas de inimigos foram concluídas!");
    }

    private void SpawnEnemyOnPath(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return;

        // Garante que a lista de caminhos esteja atualizada
        if (paths.Count == 0)
        {
            RefreshAndSanitizePaths();
        }

        if (paths.Count == 0)
        {
            // Fallback de emergência: instancia na posição do spawner
            Debug.LogWarning("[EnemySpawner] Nenhum WaypointPath foi encontrado! Instanciando na posição do Spawner.");
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            return;
        }

        if (pathMode == PathSelectionMode.AllPathsSimultaneously)
        {
            foreach (WaypointPath p in paths)
            {
                if (p != null) InstantiateEnemyOnSinglePath(enemyPrefab, p);
            }
            return;
        }

        WaypointPath selectedPath = GetNextPath();
        if (selectedPath != null)
        {
            InstantiateEnemyOnSinglePath(enemyPrefab, selectedPath);
        }
        else
        {
            // Se por algum motivo não pegou, pega o primeiro
            InstantiateEnemyOnSinglePath(enemyPrefab, paths[0]);
        }
    }

    private WaypointPath GetNextPath()
    {
        if (paths.Count == 0) return null;

        if (pathMode == PathSelectionMode.RandomPath)
        {
            int rnd = UnityEngine.Random.Range(0, paths.Count);
            return paths[rnd];
        }

        // Modo AlternatePaths
        WaypointPath p = paths[currentPathIndex % paths.Count];
        currentPathIndex++;
        return p;
    }

    private void InstantiateEnemyOnSinglePath(GameObject enemyPrefab, WaypointPath path)
    {
        if (enemyPrefab == null || path == null) return;

        Vector3 spawnPos = path.GetStartPoint();

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.SetPath(path.GetWaypoints());
        }
        else
        {
            Debug.LogWarning($"[EnemySpawner] O prefab '{enemyPrefab.name}' não possui o componente EnemyMovement!");
        }
    }
}
