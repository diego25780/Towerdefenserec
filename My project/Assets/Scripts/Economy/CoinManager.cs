using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Configuração de Moedas")]
    [SerializeField] private int startingCoins = 100;
    private int currentCoins;

    public static event Action<int> OnCoinsChanged;

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentCoins = startingCoins;
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);
        Debug.Log($"+{amount} moedas! Saldo atual: {currentCoins}");
    }

    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;

        if (HasEnoughCoins(amount))
        {
            currentCoins -= amount;
            OnCoinsChanged?.Invoke(currentCoins);
            Debug.Log($"-{amount} moedas gastas! Saldo atual: {currentCoins}");
            return true;
        }

        Debug.LogWarning("Moedas insuficientes!");
        return false;
    }
}
