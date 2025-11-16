using UnityEngine;
using System.Collections;
using TMPro; // Нужен для корутин (Coroutine)

/// <summary>
/// Управляет здоровьем игрока, получением урона и кратковременной неуязвимостью.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Время в секундах, в течение которого игрок неуязвим после получения урона.")]
    [SerializeField] private float invulnerabilityDuration = 1.0f;
    [SerializeField] private TextMeshProUGUI healthRemainingText;
    
    public float currentHealth;
    private bool isInvulnerable = false;
    private GameManager gameManager; // Ссылка на GameManager

    void Start()
    {   
        if (healthRemainingText != null)
        {
            healthRemainingText.text = $"ЗДОРОВЬЯ ОСТАЛОСЬ: {currentHealth}/{maxHealth}";
        }
        currentHealth = maxHealth;
        gameManager = FindObjectOfType<GameManager>();
    }

    /// <summary>
    /// Наносит урон игроку.
    /// </summary>
    /// <param name="damageAmount">Количество наносимого урона.</param>
    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable)
        {
            // Урон не наносится, если игрок неуязвим.
            return;
        }

        currentHealth -= damageAmount;
        if (healthRemainingText != null)
        {
            healthRemainingText.text = $"ЗДОРОВЬЯ ОСТАЛОСЬ: {currentHealth}/{maxHealth}";
        }
        
        Debug.Log($"Игрок получил урон. Текущее здоровье: {currentHealth:F0}");

        if (currentHealth <= 0)
        {
            Die();
            
        }
    }
    
    /// <summary>
    /// Логика смерти игрока.
    /// </summary>
    private void Die()
    {
        Debug.Log("Игрок мертв! Игра окончена.");
        
        // Уведомить GameManager о завершении игры
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    /// <summary>
    /// Корутина для управления периодом неуязвимости.
    /// </summary>
    private IEnumerator HandleInvulnerability()
    {
        isInvulnerable = true;
        // Опционально: добавить визуальный фидбек, как мигание
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }
}