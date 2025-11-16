using UnityEngine;

/// <summary>
/// Этот скрипт отвечает за движение врага в сторону игрока.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    // Скорость, с которой враг будет двигаться (можно настроить в Инспекторе).
    [SerializeField] private float moveSpeed = 2.5f;

    // Ссылка на компонент Transform игрока.
    private Transform playerTarget;

    // Вызывается при запуске сцены.
    void Start()
    {
        // Поиск объекта игрока по тегу "Player".
        GameObject playerObject = GameObject.FindWithTag("Player");

        // Если игрок найден, сохраняем его Transform.
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object with tag 'Player' not found in scene! Enemy cannot move.");
        }
    }

    // Вызывается каждый кадр для обновления логики.
    void Update()
    {
        // Проверяем, существует ли цель (игрок).
        if (playerTarget != null)
        {
            // 1. Вычисляем вектор направления от врага к игроку.
            Vector3 direction = (playerTarget.position - transform.position).normalized;

            // 2. Применяем движение к позиции врага.
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }
}