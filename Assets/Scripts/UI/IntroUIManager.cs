using UnityEngine;

/// <summary>
/// Управляет отображением различных спрайтов (персонажа, плитки, игрового фона) 
/// в зависимости от текущего уровня.
/// </summary>
public class IntroUIManager : MonoBehaviour
{
    [Header("Ссылки на Спрайты")]
    [SerializeField] public SpriteRenderer NormisSprite;
    [SerializeField] public SpriteRenderer TileSprite;
    [SerializeField] public CanvasGroup playerCanvasGroup;
    
    [Header("Спрайты для Уровня 1")]
    [SerializeField] public Sprite Tile1Sprite;
    [SerializeField] public Sprite NormisAmobaSprite;
    [SerializeField] public Sprite Game1Sprite;
    
    [Header("Спрайты для Уровня 2")]
    [SerializeField] public Sprite Tile2Sprite;
    [SerializeField] public Sprite NormisChervSprite;
    [SerializeField] public Sprite Game2Sprite;
    
    [Header("Спрайты для Уровня 3")]
    [SerializeField] public Sprite Tile3Sprite;
    [SerializeField] public Sprite NormisSkorpSprite;
    [SerializeField] public Sprite Game3Sprite;
    
    [Header("Спрайты для Уровня 4")]
    [SerializeField] public Sprite Tile4Sprite;
    [SerializeField] public Sprite NormisReksSprite;
    [SerializeField] public Sprite Game4Sprite;

    [Header("Текущий Уровень")]
    [Tooltip("Текущий уровень для настройки спрайтов.")]
    [SerializeField] public int level = 1;

    // --- Константы для определения типа спрайта (для повышения читаемости) ---
    private const int SPRITE_TYPE_TILE = 1;
    private const int SPRITE_TYPE_GAME = 2;
    private const int SPRITE_TYPE_NORMIS = 3;
    
    void Start()
    {
        // Изначально скрываем UI игрока, чтобы показать его после анимации
        SetAlphaGameUI(0);
        SetInteractableGameUI(false);
        
        // Устанавливаем начальные спрайты
        SetUpSpritesByLevel(NormisSprite, level, SPRITE_TYPE_NORMIS);
        SetUpSpritesByLevel(TileSprite, level, SPRITE_TYPE_TILE);
    }

    /// <summary>
    /// Устанавливает спрайт для заданного рендерера в зависимости от уровня и типа спрайта.
    /// </summary>
    /// <param name="spriteRenderer">Рендерер, в который нужно загрузить спрайт.</param>
    /// <param name="level">Текущий уровень (1-4).</param>
    /// <param name="spriteType">Тип спрайта: 1 - Плитка, 2 - Игровое поле, 3 - Нормис.</param>
    public void SetUpSpritesByLevel(SpriteRenderer spriteRenderer, int level, int spriteType)
    {
        Sprite targetSprite = null;

        // Проверяем, находится ли уровень в допустимом диапазоне
        if (level < 1 || level > 4)
        {
            Debug.LogWarning($"Попытка установить спрайты для недопустимого уровня: {level}. Используется уровень 1.");
            level = 1;
        }

        switch (spriteType)
        {
            case SPRITE_TYPE_TILE:
            {
                // Тип 1: Плитка (TileSprite)
                switch (level)
                {
                    case 1: targetSprite = Tile1Sprite; break;
                    case 2: targetSprite = Tile2Sprite; break;
                    case 3: targetSprite = Tile3Sprite; break;
                    case 4: targetSprite = Tile4Sprite; break;
                }
                break;
            }
            case SPRITE_TYPE_GAME:
            {
                // Тип 2: Игровой фон (GameSprite)
                switch (level)
                {
                    case 1: targetSprite = Game1Sprite; break;
                    case 2: targetSprite = Game2Sprite; break;
                    case 3: targetSprite = Game3Sprite; break;
                    case 4: targetSprite = Game4Sprite; break;
                }
                break;
            }
            case SPRITE_TYPE_NORMIS:
            {
                // Тип 3: Персонаж Нормис (NormisSprite)
                switch (level)
                {
                    case 1: targetSprite = NormisAmobaSprite; break;
                    case 2: targetSprite = NormisChervSprite; break;
                    case 3: targetSprite = NormisSkorpSprite; break;
                    case 4: targetSprite = NormisReksSprite; break;
                }
                break;
            }
            default:
            {
                Debug.LogError($"Неизвестный тип спрайта: {spriteType}. Проверьте вызовы SetUpSpritesByLevel.");
                return;
            }
        }
        
        if (spriteRenderer != null && targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }
        else if (spriteRenderer == null)
        {
             Debug.LogError($"SpriteRenderer для типа {spriteType} не задан в Инспекторе.");
        }
    }

    /// <summary>
    /// Устанавливает прозрачность для группы UI игрока.
    /// </summary>
    public void SetAlphaGameUI(float alpha)
    {
        if (playerCanvasGroup != null)
        {
            playerCanvasGroup.alpha = alpha;
        }
    }
    
    /// <summary>
    /// Включает/отключает взаимодействие с элементами UI игрока.
    /// </summary>
    public void SetInteractableGameUI(bool interactable)
    {
        if (playerCanvasGroup != null)
        {
            playerCanvasGroup.interactable = interactable;
            playerCanvasGroup.blocksRaycasts = interactable;
        }
    }

    /// <summary>
    /// Вызывается в конце анимации (например, Intro) для активации UI игрока.
    /// </summary>
    public void OnAnimationEnded()
    {   
        SetAlphaGameUI(1);
        SetInteractableGameUI(true);
    }
}