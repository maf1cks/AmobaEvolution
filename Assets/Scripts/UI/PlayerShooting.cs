using UnityEngine;
using UnityEngine.InputSystem; 

/// <summary>
/// Управляет логикой стрельбы игрока, включая скорострельность и инициализацию урона снаряда.
/// Должен быть прикреплен к тому же объекту, что и PlayerView.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Характеристики Атаки")]
    [Tooltip("Префаб снаряда со скриптом Projectile.cs.")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Базовый урон, который наносит снаряд (используется как INT).")]
    [SerializeField] private int baseDamage = 10;
    
    [Tooltip("Скорость атаки (выстрелов в секунду).")]
    [SerializeField] private float fireRate = 3f; 
    
    // Приватные переменные
    private float nextFireTime = 0f;
    private InputSystem_Actions playerControls;

    // В PlayerShooting должна быть своя ссылка на InputSystem_Actions
    private void Awake()
    {
        // Создаем новый экземпляр, так как PlayerView уже использует свой
        playerControls = new InputSystem_Actions(); 
    }

    private void OnEnable()
    {
        // Привязываем действие "Fire" (предполагаем, что оно есть в вашей карте действий)
        playerControls.Player.Attack.performed += OnFire;
        playerControls.Player.Enable();
    }
    
    private void OnDisable()
    {
        playerControls.Player.Attack.performed -= OnFire;
        playerControls.Player.Disable();
    }

    /// <summary>
    /// Обработчик события Fire (вызывается при нажатии кнопки)
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        // Если кнопка нажата, и прошло достаточно времени с последнего выстрела
        if (context.performed && Time.time >= nextFireTime)
        {
            Shoot();
            // Устанавливаем время для следующего возможного выстрела
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    /// <summary>
    /// Создает снаряд и инициализирует его.
    /// </summary>
    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab не назначен в PlayerShooting.cs!");
            return;
        }

        Camera _mainCamera = Camera.main;
        if (Mouse.current == null || _mainCamera == null) return;

        // 1. Получаем мировые координаты мыши
        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(
            new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0)
        );

        // 2. В одну строку: Вычисляем направление и создаем вращение.
        // LookRotation требует Vector3, поэтому Z-компонента будет использоваться для глубины, но
        // для 2D это обычно не влияет на X/Y вращение.
        Vector3 shootDirection = mouseWorldPosition - transform.position;
        // Для 2D-игры направления часто задаются Vector3.right или Vector3.up
        // Предположим, снаряд летит вправо:
        
        // ***************************************************************
        // finalDamage теперь int, что соответствует сигнатуре Initialize(Vector3, int).
        // ***************************************************************
        int finalDamage = baseDamage; 

        // 1. Создаем снаряд в текущей позиции игрока
        GameObject newProjectileObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        // 2. Получаем компонент Projectile
        Projectile newProjectile = newProjectileObject.GetComponent<Projectile>();

        if (newProjectile != null)
        {
            // 3. Инициализируем снаряд с правильным типом (int)
            newProjectile.Initialize(shootDirection, finalDamage);
        }
        else
        {
            Debug.LogError("Префаб снаряда не содержит скрипт Projectile.cs! Проверьте префаб.");
        }
    }
}
