using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerView : MonoBehaviour
{
    public float moveSpeed = 5f; 
    private Vector2 currentMovementInput; 
    
    private InputSystem_Actions playerControls; 
    
    [SerializeField] public SpriteRenderer playerSprite;
    [SerializeField] private IntroUIManager introUIManager;

    // Новые приватные переменные для хранения границ экрана
    private Vector2 screenBounds;
    private float playerWidth;
    private float playerHeight;
    
    private void Awake()
    {
        playerControls = new InputSystem_Actions();
        introUIManager.SetUpSpritesByLevel(playerSprite,introUIManager.level,3);
        
        // 1. Рассчитываем границы экрана в мировых координатах
        // Viewport (0,0) - нижний левый угол; (1,1) - верхний правый угол
        screenBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));

        // 2. Получаем половину ширины/высоты игрока, чтобы его центр
        // не вышел за край, а край спрайта оставался в границах.
        // Используем bounds.extents (половина размера)
        playerWidth = playerSprite.bounds.extents.x;
        playerHeight = playerSprite.bounds.extents.y;
    }

    private void OnEnable()
    {
        playerControls.Player.Move.started += OnMovement;
        playerControls.Player.Move.canceled += OnMovement;
        playerControls.Player.Move.performed += OnMovement;
        
        playerControls.Player.Enable();
    }
    
    private void OnDisable()
    {
        playerControls.Player.Move.started -= OnMovement;
        playerControls.Player.Move.canceled -= OnMovement;
        playerControls.Player.Move.performed -= OnMovement;
        
        playerControls.Player.Disable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            currentMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            currentMovementInput = Vector2.zero;
        }
    }

    void Update()
    {
        // === ЛОГИКА ПЕРЕМЕЩЕНИЯ ===
        Vector3 movement = new Vector3(currentMovementInput.x, currentMovementInput.y, 0f).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;

        // === ЛОГИКА ЗЕРКАЛИРОВАНИЯ (Флип) ===
        // Если движемся влево (x < 0), устанавливаем flipX в true.
        if (introUIManager.level != 3)
        {
            if (currentMovementInput.x < 0)
            {
                playerSprite.flipX = true;
            }
            // Если движемся вправо (x > 0), устанавливаем flipX в false.
            else if (currentMovementInput.x > 0)
            {
                playerSprite.flipX = false;
            }
        } else
        {
            if (currentMovementInput.x < 0)
            {
                playerSprite.flipX = false;
            }
            // Если движемся вправо (x > 0), устанавливаем flipX в false.
            else if (currentMovementInput.x > 0)
            {
                playerSprite.flipX = true;
            }
        }

        // === ЛОГИКА ОГРАНИЧЕНИЯ ГРАНИЦ ===
        Vector3 viewPos = transform.position;

        // Ограничиваем позицию X: от левого края до правого
        viewPos.x = Mathf.Clamp(viewPos.x, -screenBounds.x + playerWidth, screenBounds.x - playerWidth);

        // Ограничиваем позицию Y: от нижнего края до верхнего
        if (introUIManager.level!=4){
            viewPos.y = Mathf.Clamp(viewPos.y, -screenBounds.y + playerHeight, screenBounds.y - playerHeight);
        }
        else
        {
            viewPos.y = Mathf.Clamp(viewPos.y, -screenBounds.y + playerHeight, screenBounds.y/3 - playerHeight);
        }
        // Применяем скорректированную позицию
        transform.position = viewPos;
    }
}