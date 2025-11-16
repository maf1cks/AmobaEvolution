using UnityEngine;

/// <summary>
/// Handles projectile movement, damage application, and destruction.
/// Скрипт управляет движением снаряда, нанесением урона и его самоуничтожением.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Base Settings")]
    [Tooltip("Скорость, с которой движется снаряд.")]
    [SerializeField] private float speed = 15f;
    [Tooltip("Максимальное время жизни снаряда (для удаления, если он промахнулся).")]
    [SerializeField] private float lifetime = 3f; 

    private int damage;

    /// <summary>
    /// Initializes the projectile's direction and damage.
    /// Должен быть вызван сразу после создания снаряда.
    /// </summary>
    /// <param name="initialDirection">The normalized direction of travel.</param>
    /// <param name="baseDamage">The base damage value.</param>
    public void Initialize(Vector3 initialDirection, int baseDamage)
    {
        damage = baseDamage;
        
        float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Start the self-destruction timer
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the projectile using its speed and direction
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is an Enemy (needs tag "Enemy")
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Apply damage to the enemy
                enemy.TakeDamage(damage);
            }

            // Destroy the projectile immediately after hitting an enemy
            // Note: If you want pierce, you would skip this line for certain projectiles
            Destroy(gameObject);
        }
    }
}