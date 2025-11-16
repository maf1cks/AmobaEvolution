using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

/// <summary>
/// Управляет интерфейсом и логикой эволюции существа.
/// Использует XP как для прогресса, который затем списывается.
/// </summary>
public class EvolutionManager : MonoBehaviour
{
    [Header("Ссылки на Менеджеры")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private IntroUIManager introUIManager; // Для доступа к уровню и спрайтам

    [Header("UI Элементы")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button evolveButton;
    
    // ИСПОЛЬЗУЕМ SPRITERENDERER ДЛЯ ОТОБРАЖЕНИЯ СУЩЕСТВА (не UI)
    [SerializeField] private SpriteRenderer currentCreatureRenderer; 
    
    [Header("UI Прогресса Опыта")]
    [SerializeField] private Slider evolutionProgressBar;
    // ССЫЛКА: Компонент Image, который заполняет шкалу прогресса (Fill Area/Fill)
    [SerializeField] private Image fillImage; 
    [SerializeField] private TextMeshProUGUI xpProgressText; // Текст прогресса (75/100)

    [Header("Настройки Эволюции")]
    // EvolutionCost удален, так как списание XP производится по значению requiredXP.
    private bool isEvolving = false; // Флаг для предотвращения повторных нажатий
    
    // XP, требуемый для перехода на L1, L2, L3, L4. Индекс 0 не используется.
    private readonly int[] requiredXP = { 0, 100, 250, 500 }; 

    private void Start()
    {
        // Инициализация менеджеров, если они не заданы
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (introUIManager == null) introUIManager = FindObjectOfType<IntroUIManager>();

        if (uiManager == null || introUIManager == null)
        {
            Debug.LogError("EvolutionManager не нашел UIManager или IntroUIManager.");
            return;
        }

        // 1. Настройка кнопки "Назад"
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => uiManager.StartButtonTransition(uiManager.OpenMenuCanvas));
        }

        // 2. Настройка кнопки "Эволюция"
        if (evolveButton != null)
        {
            evolveButton.onClick.AddListener(AttemptEvolution);
        }

        // 3. Установка начального отображения (текущий спрайт и шкала XP)
        UpdateEvolutionUI();
        
        // --- Улучшенная проверка: если fillImage не назначен, пытаемся найти его автоматически ---
        if (fillImage == null && evolutionProgressBar != null)
        {
            // 1. Пытаемся получить компонент Image из Slider.fillRect (предпочтительный путь)
            if (evolutionProgressBar.fillRect != null)
            {
                fillImage = evolutionProgressBar.fillRect.GetComponent<Image>();
            }

            // 2. Если не найдено, ищем по стандартному пути в иерархии Unity (Fill Area/Fill)
            if (fillImage == null)
            {
                 Transform fillTransform = evolutionProgressBar.transform.Find("Fill Area/Fill");
                 if (fillTransform != null)
                 {
                     fillImage = fillTransform.GetComponent<Image>();
                 }
            }
             
            if (fillImage == null)
            {
                // Напоминание о необходимости ручной настройки, если автоматическое назначение не сработало.
                Debug.LogWarning("Fill Image для шкалы прогресса не назначен и не найден автоматически. Пожалуйста, назначьте компонент 'Image' из объекта 'Fill' вручную в Inspector.");
            }
        }
    }

    /// <summary>
    /// Обновляет отображение текущего существа и шкалы прогресса XP.
    /// </summary>
    private void UpdateEvolutionUI()
    {
        if (introUIManager == null || uiManager == null) return;
        
        // --- ПОЛУЧАЕМ ТЕКУЩИЙ XP ---
        int currentXP = uiManager.GetCurrentXP();

        int currentLevel = introUIManager.level;
        
        // --- 1. Определяем, возможна ли эволюция (если уровень не максимальный) ---
        bool canEvolve = false;
        if (currentLevel < 4)
        {
            int required = requiredXP[currentLevel];
            
            // Проверяем: достаточно ли XP для требования прогресса (required)
            bool hasEnoughXPForRequirement = currentXP >= required; 
            
            canEvolve = hasEnoughXPForRequirement;
        }

        // --- 2. Обновляем отображение спрайта (следующее существо) и его яркость ---
        SetNextCreatureDisplay(currentLevel, canEvolve);
        
        // --- 3. Обновление шкалы опыта ---
        UpdateXPProgressBar(currentLevel, currentXP);
        
        // --- 4. Блокировка кнопки "Эволюция" ---
        if (currentLevel >= 4)
        {
            if (evolveButton != null) evolveButton.interactable = false;
        }
        else
        {
            if (evolveButton != null) evolveButton.interactable = canEvolve;
        }
    }

    private Sprite GetCreatureSpriteForLevel(int level)
    {
        if (introUIManager == null) return null;
        
        switch (level)
        {
            case 1: return introUIManager.NormisAmobaSprite;
            case 2: return introUIManager.NormisChervSprite;
            case 3: return introUIManager.NormisSkorpSprite;
            case 4: return introUIManager.NormisReksSprite;
            default: return null; 
        }
    }

    public void UpdateXP()
    {
        UpdateEvolutionUI();
    }

    private void SetNextCreatureDisplay(int currentLevel, bool isReadyToEvolve)
    {
        if (currentCreatureRenderer == null) return;
        
        int displayLevel = (currentLevel >= 4) ? 4 : currentLevel + 1;
        
        Sprite nextSprite = GetCreatureSpriteForLevel(displayLevel);

        if (nextSprite != null)
        {
            currentCreatureRenderer.sprite = nextSprite;
            currentCreatureRenderer.gameObject.SetActive(true);

            Color spriteColor = Color.white;
            
            if (currentLevel < 4 && !isReadyToEvolve)
            {
                spriteColor.a = 0.6f; // Тусклый
            }
            else
            {
                spriteColor.a = 1.0f; // Обычный
            }
            
            currentCreatureRenderer.color = spriteColor;
        }
        else
        {
            currentCreatureRenderer.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Обновляет шкалу прогресса опыта и цвет заполнения.
    /// Цвет заполнения: Желтый (обычный), Зеленый (готов к эволюции), Серый (максимум).
    /// </summary>
    private void UpdateXPProgressBar(int currentLevel, int currentXP)
    {
        if (evolutionProgressBar == null || xpProgressText == null) return;
        
        // --- 1. Устанавливаем базовый цвет заполнения - ЖЕЛТЫЙ ---
        Color fillColor = Color.yellow; 

        if (currentLevel >= 4)
        {
            evolutionProgressBar.gameObject.SetActive(false);
            xpProgressText.text = "Эволюция завершена!";
            xpProgressText.color = Color.gray; // Текст становится серым
            fillColor = Color.gray; // Заполнение становится серым
        }
        else
        {
            evolutionProgressBar.gameObject.SetActive(true);
            
            // Требования для следующего уровня
            int required = requiredXP[currentLevel]; 

            // Обновляем Slider
            evolutionProgressBar.minValue = 0;
            evolutionProgressBar.maxValue = required;
            evolutionProgressBar.value = currentXP; 

            // Обновляем текст в формате (текущий XP) / (требуемый XP)
            xpProgressText.text = $"{currentXP} / {required}";
            // Цвет текста (цифр) установлен на белый для контраста.
            xpProgressText.color = Color.black; 

            // --- 2. Визуальный индикатор готовности XP (только шкала) ---
            if (currentXP >= required)
            {
                fillColor = Color.green; // Зеленый, когда готово
            }
            // else { fillColor остается Color.yellow }
        }
        
        // Устанавливаем цвет заполненной части шкалы
        if (fillImage != null)
        {
            fillImage.color = fillColor;
        }
        else
        {
            // Здесь будет выведено предупреждение, если fillImage не назначен.
            Debug.LogWarning("Fill Image для шкалы прогресса не назначен! Цвет не изменен.");
        }
    }

    /// <summary>
    /// Добавляет указанное количество опыта (XP).
    /// </summary>

    /// <summary>
    /// Проверяет условия и запускает эволюцию.
    /// Эволюция требует только накопления необходимого XP, который затем списывается.
    /// </summary>
    private void AttemptEvolution()
    {
        if (isEvolving || introUIManager.level >= 4) return;
        
        StopAllCoroutines(); 
        
        // Получаем XP из центрального хранилища для проверки
        int currentXP = uiManager.GetCurrentXP();
        
        int required = requiredXP[introUIManager.level];
        
        // Проверка: достаточно ли XP для требования (required)
        bool hasEnoughXPForRequirement = currentXP >= required; 

        if (hasEnoughXPForRequirement)
        {
            StartCoroutine(PerformEvolutionSequence());
        }
        else
        {
            Debug.LogWarning($"Попытка эволюции заблокирована: Недостаточно XP. Требуется: {required}, Доступно: {currentXP}");
        }
    }

    /// <summary>
    /// Корутина для выполнения анимации эволюции и обновления состояния.
    /// </summary>
    private IEnumerator PerformEvolutionSequence()
    {
        if (uiManager == null) yield break;
        
        isEvolving = true;
        evolveButton.interactable = false;
        
        // 1. Получаем требуемое количество XP для ТЕКУЩЕГО уровня (перед эволюцией)
        int xpDeduction = requiredXP[introUIManager.level];
        
        // --- Списание ---
        // Списываем XP, необходимый для требования (например, -100).
        // Дополнительная стоимость EvolutionCost удалена.
        uiManager.AddXP(-xpDeduction); 
        
        // --- Симуляция анимации эволюции ---
        yield return new WaitForSeconds(1.5f); 

        // 3. Увеличиваем уровень
        introUIManager.level++;
        
        // 4. Обновляем все спрайты в главном меню/игре
        introUIManager.SetUpSpritesByLevel(introUIManager.NormisSprite, introUIManager.level, 3);
        introUIManager.SetUpSpritesByLevel(introUIManager.TileSprite, introUIManager.level, 1);
        
        // 5. Обновляем UI эволюции (полосу прогресса и спрайт)
        UpdateEvolutionUI();

        Debug.Log($"Эволюция успешна! Теперь уровень {introUIManager.level}! Списано {xpDeduction} XP. Остаток XP: {uiManager.GetCurrentXP()}");
        
        isEvolving = false;
        UpdateEvolutionUI(); 
    }
}