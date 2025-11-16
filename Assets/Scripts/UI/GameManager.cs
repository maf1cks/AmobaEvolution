using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Управляет основным игровым циклом, уровнями (от 1 до 50) и прогрессией сложности.
/// Координирует спаунер и игрока.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Ссылки на Менеджеры и Компоненты")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private IntroUIManager introUIManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TextMeshProUGUI healthRemainingText;
    
    [Header("Настройки Прогрессии")]
    // Текущий игровой уровень (от 1 до 50), не путать с уровнем эволюции (1-4).
    [SerializeField] public int currentLevel = 1; 
    [Tooltip("Общее количество врагов, которые должны быть уничтожены для завершения уровня.")]
    [SerializeField] private int baseEnemiesPerLevel = 20; 
    [Tooltip("Дополнительное количество врагов на каждый уровень сложности.")]
    [SerializeField] private int enemyIncreasePerLevel = 5; 

    // UI для отображения уровня
    [Header("Игровой UI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;

    private int enemiesKilledInLevel = 0;
    private int totalEnemiesToKill;
    private bool isGameActive = false;

    private void Start()
    {
        // Инициализация, если ссылки не заданы в инспекторе
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (introUIManager == null) introUIManager = FindObjectOfType<IntroUIManager>();
        if (enemySpawner == null) enemySpawner = FindObjectOfType<EnemySpawner>();
        
        // Устанавливаем начальный текст
        UpdateLevelUI();
    }
    
    /// <summary>
    /// Запускает игру и текущий уровень. Вызывается из UIManager при нажатии кнопки "Игра".
    /// </summary>
    public void StartGame()
    {
        if (isGameActive) return;
        
        Debug.Log($"Начало игры на Уровне {currentLevel}.");
        isGameActive = true;
        playerHealth.currentHealth = 100;
        
        // 1. Сброс счетчиков
        enemiesKilledInLevel = 0;
        
        // 2. Расчет общего количества врагов для текущего уровня
        totalEnemiesToKill = baseEnemiesPerLevel + (currentLevel - 1) * enemyIncreasePerLevel;
        
        // 3. Настройка сложности врагов
        if (enemySpawner != null)
        {
            // Спаунер будет использовать currentLevel для масштабирования сложности врагов.
            enemySpawner.StartSpawning(currentLevel); 
        }

        // 4. Обновление UI
        UpdateLevelUI();
        UpdateEnemiesRemainingUI();
    }
    
    /// <summary>
    /// Вызывается врагом при его уничтожении.
    /// </summary>
    public void EnemyKilled(int xpReward, int coinReward, int a)
    {   
        if (!isGameActive) return;
        enemiesKilledInLevel+=1*a;
        // NOTE: Предполагается, что uiManager.AddXP и uiManager.AddCoins существуют.
        // uiManager.AddXP(xpReward);
        // uiManager.AddCoins(coinReward);
        uiManager.AddCoins(coinReward);
        uiManager.AddXP(xpReward);
        UpdateEnemiesRemainingUI();
        
        // Проверка завершения уровня
        if (enemiesKilledInLevel >= totalEnemiesToKill)
        {
            
            CompleteLevel();
        }
    }

    /// <summary>
    /// Завершение текущего уровня и переход к следующему.
    /// </summary>
    private void CompleteLevel()
    {
        isGameActive = false;
        
        // 1. Остановка всех вражеских действий
        if (enemySpawner != null) enemySpawner.StopSpawning();
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        // 2. Пройтись по полученному массиву и удалить каждый объект.
        foreach (Enemy enemy in allEnemies)
        {
            // Destroy - это статический метод, который удаляет объект.
            // Мы удаляем весь игровой объект (GameObject), к которому прикреплен скрипт Enemy.
            Destroy(enemy.gameObject);
        }
        // 2. Увеличение уровня
        currentLevel++;
        if (currentLevel > 50) currentLevel = 50; // Максимальный уровень
        
        Debug.Log($"Уровень {currentLevel - 1} завершен! Подготовка к Уровню {currentLevel}.");
        
        // 3. Используем фейдер UIManager для перехода и показываем меню
        if (uiManager != null)
        {
            uiManager.StartButtonTransition(() =>
            {
                enemiesRemainingText.text = null;
                healthRemainingText.text = null;
                uiManager.OpenWinScreen(enemiesKilledInLevel);
                uiManager.OpenMenuCanvas(); 
            });
        }
    }

    /// <summary>
    /// *** НОВЫЙ МЕТОД: Обработка окончания игры при смерти игрока. ***
    /// Вызывается скриптом PlayerHealth.cs.
    /// </summary>
    public void GameOver()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        // 1. Остановка всех вражеских действий
        if (enemySpawner != null) enemySpawner.StopSpawning();
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        // 2. Пройтись по полученному массиву и удалить каждый объект.
        foreach (Enemy enemy in allEnemies)
        {
            // Destroy - это статический метод, который удаляет объект.
            // Мы удаляем весь игровой объект (GameObject), к которому прикреплен скрипт Enemy.
            Destroy(enemy.gameObject);
        }
        Debug.Log("Game Over: Игрок погиб.");
        
        // 2. Показываем экран "Game Over"
        if (uiManager != null)
        {
            enemiesRemainingText.text = null;
            healthRemainingText.text = null;
            uiManager.OpenGameOverScreen(enemiesKilledInLevel); 
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"УРОВЕНЬ: {currentLevel}";
        }
    }
    
    private void UpdateEnemiesRemainingUI()
    {
        if (enemiesRemainingText != null)
        {
            int remaining = Mathf.Max(0, totalEnemiesToKill - enemiesKilledInLevel);
            enemiesRemainingText.text = $"ВРАГОВ ОСТАЛОСЬ: {remaining}/{totalEnemiesToKill}";
        }
    }
}