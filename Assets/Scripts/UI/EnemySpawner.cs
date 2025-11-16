using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Отвечает за периодический спаун врагов в разных точках
/// в зависимости от текущего уровня сложности.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Префабы и Ссылки")]
    [Tooltip("Префаб врага, который будет спауниться.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("Ссылка на Transform игрока, к которому будут двигаться враги.")]
    [SerializeField] private Transform playerTransform;

    [SerializeField] private IntroUIManager introUIManager;
    public List<LevelEnemy> enemyesSprites;
    [Header("Настройки Спауна")]
    [Tooltip("Базовое время между спаунами (в секундах).")]
    [SerializeField] private float baseSpawnInterval = 2f;
    [Tooltip("На сколько процентов уменьшается интервал спауна за каждый уровень.")]
    [SerializeField] private float intervalReductionPerLevelPercent = 2f; // 2%

    [Header("Зона Спауна")]
    [Tooltip("Радиус от игрока, за пределами которого могут появляться враги.")]
    [SerializeField] private float spawnRadius = 15f;

    private bool isSpawning = false;
    private int currentLevelDifficulty = 1;
    private Coroutine spawnRoutine;

    private void Start()
    {
        // Попытка найти игрока, если не задан в Инспекторе
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Игрок не найден! Установите тег 'Player' или перетащите его в Инспекторе.");
            }
        }
    }

    /// <summary>
    /// Начинает процесс спауна, вызывается GameManager при начале уровня.
    /// </summary>
    /// <param name="level">Текущий уровень сложности, масштабирует скорость спауна.</param>
    public void StartSpawning(int level)
    {
        if (playerTransform == null) return;
        
        currentLevelDifficulty = level;
        isSpawning = true;
        
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// Останавливает процесс спауна, вызывается GameManager при завершении уровня или Game Over.
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    /// <summary>
    /// Корутина, управляющая циклом спауна.
    /// </summary>
    private IEnumerator SpawnEnemiesRoutine()
    {
        while (isSpawning)
        {
            // Расчет интервала спауна, уменьшение со сложностью
            float reductionFactor = 1f - (currentLevelDifficulty - 1) * (intervalReductionPerLevelPercent / 100f);
            float currentInterval = Mathf.Max(0.5f, baseSpawnInterval * reductionFactor); // Минимальный интервал - 0.5 сек

            SpawnEnemy();
            
            yield return new WaitForSeconds(currentInterval);
        }
    }

    /// <summary>
    /// Создает одного врага в случайной точке за пределами экрана.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        // Выбираем случайную позицию в круге вокруг игрока
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

        // Создаем врага
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        Sprite sprite = null;
        newEnemy.GetComponent<Enemy>().Initialize(currentLevelDifficulty);
        newEnemy.GetComponent<SpriteRenderer>().sprite = enemyesSprites[introUIManager.level-1].randomSprite[Random.Range(0, enemyesSprites.Count-1)];
        
        // Передаем ссылку на игрока, чтобы враг мог двигаться (потребуется EnemyMovement.cs)
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.SetTarget(playerTransform);
        }
        else
        {
            Debug.LogWarning("EnemyMovement компонент не найден на префабе врага.");
        }
    }
}
[System.Serializable]
public class LevelEnemy
{
    public List<Sprite> randomSprite;
}