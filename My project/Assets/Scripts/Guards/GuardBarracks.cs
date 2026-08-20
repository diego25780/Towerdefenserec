using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardBarracks : MonoBehaviour
{
    public static GuardBarracks SelectedBarracks { get; private set; }

    [Header("Configuração dos Guardas")]
    [SerializeField] private GameObject guardPrefab;
    [SerializeField] private int guardCount = 2;
    [SerializeField] private Transform rallyPoint;
    [SerializeField] private float rallyFormationRadius = 0.6f;
    [SerializeField] private float respawnTime = 6f;

    [Header("Evolução de Dano")]
    [SerializeField] private int baseDamageCost = 40;
    [SerializeField] private float damageCostMultiplier = 1.5f;
    [SerializeField] private float damageBonusPerLevel = 6f;

    [Header("Evolução de Vida")]
    [SerializeField] private int baseHealthCost = 35;
    [SerializeField] private float healthCostMultiplier = 1.5f;
    [SerializeField] private float healthBonusPerLevel = 35f;

    private int damageLevel = 1;
    private int healthLevel = 1;
    private List<Guard> activeGuards = new List<Guard>();

    public int DamageLevel => damageLevel;
    public int HealthLevel => healthLevel;
    public int CurrentDamageUpgradeCost => Mathf.RoundToInt(baseDamageCost * Mathf.Pow(damageCostMultiplier, damageLevel - 1));
    public int CurrentHealthUpgradeCost => Mathf.RoundToInt(baseHealthCost * Mathf.Pow(healthCostMultiplier, healthLevel - 1));

    public static event Action<GuardBarracks> OnBarracksSelected;
    public event Action OnUpgradesChanged;

    private void Awake()
    {
        SelectedBarracks = this;
    }

    private void Start()
    {
        SpawnAllGuards();
        OnBarracksSelected?.Invoke(this);
    }

    private void OnMouseDown()
    {
        SelectedBarracks = this;
        OnBarracksSelected?.Invoke(this);
    }

    private void SpawnAllGuards()
    {
        if (guardPrefab == null) return;

        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;

        for (int i = 0; i < guardCount; i++)
        {
            Vector3 offset = GetFormationOffset(i, guardCount);
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
    }

    private Vector3 GetFormationOffset(int index, int total)
    {
        if (total <= 1) return Vector3.zero;
        float angle = index * (360f / total) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * rallyFormationRadius;
    }

    public void OnGuardDied(Guard deadGuard)
    {
        activeGuards.Remove(deadGuard);
        StartCoroutine(RespawnGuardRoutine());
    }

    private IEnumerator RespawnGuardRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        if (guardPrefab == null) yield break;

        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;
        int nextIndex = activeGuards.Count;
        Vector3 spawnPos = center + GetFormationOffset(nextIndex, guardCount);

        GameObject guardObj = Instantiate(guardPrefab, transform.position, Quaternion.identity);
        Guard guard = guardObj.GetComponent<Guard>();

        if (guard != null)
        {
            guard.Setup(this, spawnPos);
            ApplyUpgradesToGuard(guard);
            activeGuards.Add(guard);
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

        for (int i = 0; i < activeGuards.Count; i++)
        {
            if (activeGuards[i] != null)
            {
                Vector3 newPos = newRallyPosition + GetFormationOffset(i, guardCount);
                activeGuards[i].SetGuardPosition(newPos);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 center = rallyPoint != null ? rallyPoint.position : transform.position + Vector3.down * 1.5f;
        Gizmos.DrawWireSphere(center, rallyFormationRadius);
        Gizmos.DrawLine(transform.position, center);
    }
}
