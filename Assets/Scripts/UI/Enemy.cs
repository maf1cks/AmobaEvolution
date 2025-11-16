using UnityEngine;

/// <summary>
/// Базовый класс для врагов. Отвечает за здоровье, смерть, награды и обработку столкновений с игроком.
/// Инициализирует характеристики врага, масштабируя их в зависимости от игрового уровня.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Базовые характеристики")]
    [SerializeField] private float baseHealth = 20f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private int baseXPReward = 1;
    [SerializeField] private int baseCoinReward = 2;
    
    private float currentHealth;
    // Удалены переменные, связанные со скоростью и playerTransform, так как они перемещены в EnemyMovement.cs
    
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Поиск игрока и логика движения теперь находится в отдельном скрипте EnemyMovement.cs
    }

    /// <summary>
    /// Инициализирует врага, масштабируя его характеристики в зависимости от игрового уровня.
    /// Вызывается EnemySpawner при создании врага.
    /// </summary>
    public void Initialize(int gameLevel)
    {
        // 1. Расчет масштабирования сложности (линейная прогрессия)
        // Пример: +15% здоровья за каждый уровень
        float healthMultiplier = 1f + (gameLevel - 1) * 0.15f; 
        
        currentHealth = baseHealth * healthMultiplier;
        
        // Если у вас есть EnemyMovement.cs, вы можете передать в него масштабированную скорость
        // EnemyMovement movement = GetComponent<EnemyMovement>();
        // if (movement != null)
        // {
        //     movement.InitializeSpeed(1f + (gameLevel - 1) * 0.05f); 
        // }
    }

    /// <summary>
    /// Нанесение урона врагу (вызывается снарядом Projectile).
    /// </summary>
    public void TakeDamage(int amount) // Изменил тип на int, так как урон от снаряда, скорее всего, целый.
    {
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Die(1);
        }
    }
    
    private void Die(int a)
    {   
        // Уведомляем GameManager об убийстве, чтобы он обновил счетчики
        if (gameManager != null)
        {
            gameManager.EnemyKilled(a*baseXPReward, a*baseCoinReward,a);
            
        }
        
        Destroy(gameObject);
    }
    
    // Обработка столкновения с игроком (нанесение урона)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Получаем компонент здоровья игрока и наносим урон
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(baseDamage);
            }
            
            // Уничтожаем врага после атаки
            Die(0); 
        }
    }
}