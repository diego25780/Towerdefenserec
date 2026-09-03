using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardBarracks : MonoBehaviour
{
    public static GuardBarracks SelectedBarracks { get; private set; }

    [Header("Configuração dos Guardas")]
    [SerializeField] private GameObject guardPrefab;
    [SerializeField] private int initialGuardCount = 2;
    [SerializeField] private int maxGuardCount = 5;
    [SerializeField] private int buyGuardBaseCost = 50;
    [SerializeField] private float buyGuardCostMultiplier = 1.3f;
    [SerializeField] private Transform rallyPoint;
    [SerializeField] private float rallyFormationRadius = 0.7f;
    [SerializeField] private float respawnTime = 5f;

    [Header("Evolução de Dano")]
    [SerializeField] private int baseDamageCost = 40;
    [SerializeField] private float damageCostMultiplier = 1.5f;
    [SerializeField] private float damageBonusPerLevel = 6f;

    [Header("Evolução de Vida")]
    [SerializeField] private int baseHealthCost = 35;
    [SerializeField] private float healthCostMultiplier = 1.5f;
    [SerializeField] private float healthBonusPerLevel = 35f;

    private int currentGuardLimit;
    private int damageLevel = 1;
    private int healthLevel = 1;
    private List<Guard> activeGuards = new List<Guard>();

    public int DamageLevel => damageLevel;
    public int HealthLevel => healthLevel;
    public int CurrentGuardCount => activeGuards.Count;
    public int CurrentGuardLimit => currentGuardLimit;
    public int MaxGuardCount => maxGuardCount;
    public bool CanBuyMoreGuards => currentGuardLimit < maxGuardCount;

    public int BuyGuardCost => Mathf.RoundToInt(buyGuardBaseCost * Mathf.Pow(buyGuardCostMultiplier, currentGuardLimit - initialGuardCount));
    public int CurrentDamageUpgradeCost => Mathf.RoundToInt(baseDamageCost * Mathf.Pow(damageCostMultiplier, damageLevel - 1));
    public int CurrentHealthUpgradeCost => Mathf.RoundToInt(baseHealthCost * Mathf.Pow(healthCostMultiplier, healthLevel - 1));

    public static event Action<GuardBarracks> OnBarracksSelected;
    public event Action OnUpgradesChanged;
    public event Action OnGuardsChanged;

    private void Awake()
    {
        SelectedBarracks = this;
        currentGuardLimit = initialGuardCount;
    }

    private void Start()
    {
        if (rallyPoint == null)
        {
            // Cria um ponto de encontro automático se nenhum foi definido
            GameObject rpObj = new GameObject("AutoRallyPoint");
            rpObj.transform.SetParent(transform);
            rpObj.transform.position = transform.position + Vector3.down * 1.5f;
            rallyPoint = rpObj.transform;
        }

        SpawnAllGuards();
        OnBarracksSelected?.Invoke(this);
    }

    private void OnMouseDown()
    {
        SelectedBarracks = this;
        OnBarracksSelected?.Invoke(this);
    }

    public void SpawnAllGuards()
    {
        if (guardPrefab == null)
        {
            Debug.LogWarning($"[GuardBarracks] Guard Prefab não foi atribuído na barraca {gameObject.name}!");
            return;
        }

        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;

        for (int i = 0; i < currentGuardLimit; i++)
        {
            SpawnSingleGuard(i, currentGuardLimit, center);
        }

        OnGuardsChanged?.Invoke();
    }

    private void SpawnSingleGuard(int index, int total, Vector3 center)
    {
        Vector3 offset = GetFormationOffset(index, total);
        Vector3 spawnPos = center + offset;

        GameObject guardObj = Instantiate(guardPrefab, spawnPos, Quaternion.identity);
        Guard guard = guardObj.GetComponent<Guard>();

        if (guard != null)
        {
            guard.Setup(this, spawnPos);
            ApplyUpgradesToGuard(guard);
            activeGuards.Add(guard);
        }
    }

    private Vector3 GetFormationOffset(int index, int total)
    {
        if (total <= 1) return Vector3.zero;
        float angle = index * (360f / total) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * rallyFormationRadius;
    }

    public bool BuyExtraGuard()
    {
        if (!CanBuyMoreGuards)
        {
            Debug.LogWarning("Limite máximo de guardas atingido para esta barraca!");
            return false;
        }

        int cost = BuyGuardCost;

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            currentGuardLimit++;
            Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;
            SpawnSingleGuard(activeGuards.Count, currentGuardLimit, center);

            // Reorganiza posições dos guardas na formação
            UpdateGuardPositions();

            OnGuardsChanged?.Invoke();
            OnUpgradesChanged?.Invoke();
            Debug.Log($"Novo guarda recrutado com sucesso! Total: {currentGuardLimit}/{maxGuardCount}");
            return true;
        }

        Debug.LogWarning("Moedas insuficientes para recrutar mais um guarda!");
        return false;
    }

    public void OnGuardDied(Guard deadGuard)
    {
        activeGuards.Remove(deadGuard);
        OnGuardsChanged?.Invoke();
        StartCoroutine(RespawnGuardRoutine());
    }

    private IEnumerator RespawnGuardRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        if (guardPrefab == null || activeGuards.Count >= currentGuardLimit) yield break;

        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;
        int nextIndex = activeGuards.Count;
        Vector3 spawnPos = center + GetFormationOffset(nextIndex, currentGuardLimit);

        GameObject guardObj = Instantiate(guardPrefab, transform.position, Quaternion.identity);
        Guard guard = guardObj.GetComponent<Guard>();

        if (guard != null)
        {
            guard.Setup(this, spawnPos);
            ApplyUpgradesToGuard(guard);
            activeGuards.Add(guard);
            OnGuardsChanged?.Invoke();
        }
    }

    private void UpdateGuardPositions()
    {
        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;
        for (int i = 0; i < activeGuards.Count; i++)
        {
            if (activeGuards[i] != null)
            {
                Vector3 newPos = center + GetFormationOffset(i, currentGuardLimit);
                activeGuards[i].SetGuardPosition(newPos);
            }
        }
    }

    public bool UpgradeDamage()
    {
        int cost = CurrentDamageUpgradeCost;

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            damageLevel++;
            ApplyUpgradesToAllGuards();
            OnUpgradesChanged?.Invoke();
            Debug.Log($"Guardas evoluídos para Nível de Dano {damageLevel}!");
            return true;
        }

        Debug.LogWarning("Não há moedas suficientes para evoluir o dano dos guardas!");
        return false;
    }

    public bool UpgradeHealth()
    {
        int cost = CurrentHealthUpgradeCost;

        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            healthLevel++;
            ApplyUpgradesToAllGuards();
            OnUpgradesChanged?.Invoke();
            Debug.Log($"Guardas evoluídos para Nível de Vida {healthLevel}!");
            return true;
        }

        Debug.LogWarning("Não há moedas suficientes para evoluir a vida dos guardas!");
        return false;
    }

    private void ApplyUpgradesToAllGuards()
    {
        foreach (Guard guard in activeGuards)
        {
            if (guard != null && !guard.IsDead)
            {
                ApplyUpgradesToGuard(guard);
            }
        }
    }

    private void ApplyUpgradesToGuard(Guard guard)
    {
        float extraHealth = (healthLevel - 1) * healthBonusPerLevel;
        float extraDamage = (damageLevel - 1) * damageBonusPerLevel;
        guard.UpgradeStats(extraHealth, extraDamage);
    }

    public void SetRallyPoint(Vector3 newRallyPosition)
    {
        if (rallyPoint != null)
        {
            rallyPoint.position = newRallyPosition;
        }
        UpdateGuardPositions();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;
        Gizmos.DrawWireSphere(center, rallyFormationRadius);
        Gizmos.DrawLine(transform.position, center);
    }
}
