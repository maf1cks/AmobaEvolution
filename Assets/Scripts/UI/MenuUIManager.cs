using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections; // Обязательно для использования Coroutines
using System.Globalization; // Для форматирования валюты
using System; // Добавлено для использования делегата Action

public class UIManager : MonoBehaviour
{   
    [Header("Элементы для Игры")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private SpriteRenderer gameTile;
    [SerializeField] private IntroUIManager introUIManager;
    [SerializeField] private Canvas menuCanvas;
    // Все Canvas, которые будут активироваться
    [SerializeField] private GameObject casinoCanvas;
    [SerializeField] private GameObject magazinCanvas;
    [SerializeField] private GameObject arenaCanvas;
    [SerializeField] private GameObject evolutionCanvas;
    [SerializeField] private GameObject zadanieCanvas;
    
    [Header("Системные Элементы Анимации")]
    // Ссылка на компонент Image (круглая черная картинка), который будет фейдером
    [SerializeField] private Image faderImage;
    // Длительность одной фазы анимации (расширение ИЛИ схлопывание)
    [SerializeField] private float fadeDuration = 1.5f; 
    // Целевой масштаб, который гарантированно покроет весь экран (например, 50)
    [SerializeField] private float targetScale = 50f; 
    // Время задержки при полностью черном экране
    [SerializeField] private float blackScreenDelay = 0.5f; 

    [Header("Валюта (Currency UI)")]
    [SerializeField] private TextMeshProUGUI coinCount;
    private int currentCoins = 0; 
    [SerializeField] private TextMeshProUGUI pointCount;
    private int currentCrystals = 0;
    
    [Header("Кнопки (Main Menu Buttons)")]
    [SerializeField] private Button magazinButton;
    [SerializeField] private Button arenaButton;
    [SerializeField] private Button zadanieButton;
    [SerializeField] private Button evolutionButton;
    [SerializeField] private Button gameButton;
    [SerializeField] private Button casinoButton;

    private void Start()
    {
        // Инициализация при запуске (обычно считывается из PlayerPrefs или сохранения)
        currentCoins = 100;
        currentCrystals = 5;
        
        UpdateAllCurrencyDisplay();
        
        // --- УНИВЕРСАЛЬНАЯ ПОДПИСКА КНОПОК НА ПЕРЕХОД ---
        // Все кнопки теперь запускают StartButtonTransition, передавая функцию-действие (Action), 
        // которое должно произойти, когда экран полностью затемнен.
        if (magazinButton != null) magazinButton.onClick.AddListener(() => StartButtonTransition(OpenMagazinCanvas));
        if (arenaButton != null) arenaButton.onClick.AddListener(() => StartButtonTransition(OpenArenaCanvas));
        if (zadanieButton != null) zadanieButton.onClick.AddListener(() => StartButtonTransition(OpenZadanieCanvas));
        if (evolutionButton != null) evolutionButton.onClick.AddListener(() => StartButtonTransition(OpenEvolutionCanvas));
        if (gameButton != null) gameButton.onClick.AddListener(() => StartButtonTransition(OpenGameCanvas)); // Действие для запуска игры
        if (casinoButton != null) casinoButton.onClick.AddListener(() => StartButtonTransition(OpenCasinoCanvas));
        
        // Убеждаемся, что фейдер изначально скрыт
        if (faderImage != null)
        {
            faderImage.gameObject.SetActive(false);
            faderImage.transform.localScale = Vector3.one * 0.01f;
        }
    }
    
    // --- ДЕЙСТВИЯ, ВЫПОЛНЯЕМЫЕ В СЕРЕДИНЕ ПЕРЕХОДА (ОТКРЫТИЕ CANVAS) ---
    // Эти функции содержат логику смены Canvas и вызываются во время затемнения.

    private void OpenMagazinCanvas()
    {
        // Скрываем все остальные Canvas, если они были активны
        HideAllCanvases();
        if (magazinCanvas != null) magazinCanvas.SetActive(true);
        if (menuCanvas != null) menuCanvas.enabled = false;
        Debug.Log("Действие: МАГАЗИН - открыт интерфейс магазина.");
    }

    private void OpenArenaCanvas()
    {
        HideAllCanvases();
        if (arenaCanvas != null) arenaCanvas.SetActive(true);
        if (menuCanvas != null) menuCanvas.enabled = false;
        Debug.Log("Действие: АРЕНА - запуск режима Арены.");
    }

    private void OpenZadanieCanvas()
    {   
        HideAllCanvases();
        if (zadanieCanvas != null) zadanieCanvas.SetActive(true);
        if (menuCanvas != null) menuCanvas.enabled = false;
        Debug.Log("Действие: ЗАДАНИЕ - открыт интерфейс ежедневных заданий.");
    }
    
    private void OpenEvolutionCanvas()
    {   
        HideAllCanvases();
        if (evolutionCanvas != null) evolutionCanvas.SetActive(true);
        if (menuCanvas != null) menuCanvas.enabled = false;
        Debug.Log("Действие: ЭВОЛЮЦИЯ - открыт интерфейс эволюции персонажей/юнитов.");
    }

    private void OpenCasinoCanvas()
    {
        HideAllCanvases();
        if (casinoCanvas != null) casinoCanvas.SetActive(true);
        if (menuCanvas != null) menuCanvas.enabled = false;
        Debug.Log("Действие: КАЗИНО - открыт интерфейс казино/лотереи.");
    }

    // Действие для запуска самой игры (Game)
    private void OpenGameCanvas()
    {
        // Скрываем все остальные Canvas
        HideAllCanvases();

        // Активируем префаб игрока
        if (playerPrefab != null) playerPrefab.SetActive(true);
        
        // Отключаем Canvas меню
        if (menuCanvas != null)
        {
            menuCanvas.enabled = false;
        }
        
        // Установка спрайтов игрового поля
        if (introUIManager != null && gameTile != null)
        {
             introUIManager.SetUpSpritesByLevel(gameTile, introUIManager.level, 2);
        }
        
        Debug.Log("Действие: GAME - Меню отключено, игровой процесс активирован.");
    }
    
    /// <summary>
    /// Действие, которое скрывает текущий активный Canvas и показывает главное меню.
    /// Это будет использоваться кнопкой "Назад" в других Canvas.
    /// </summary>
    public void OpenMenuCanvas()
    {
        // 1. Скрываем все игровые Canvas
        HideAllCanvases();

        // 2. Деактивируем игрока, если он был активен
        if (playerPrefab != null) playerPrefab.SetActive(false);
        
        // 3. Активируем Canvas меню
        if (menuCanvas != null)
        {
            menuCanvas.enabled = true;
        }

        Debug.Log("Действие: МЕНЮ - Скрыты все Canvas, активировано главное меню.");
    }

    /// <summary>
    /// Вспомогательная функция для скрытия всех дополнительных Canvas.
    /// </summary>
    private void HideAllCanvases()
    {
        if (casinoCanvas != null) casinoCanvas.SetActive(false);
        if (magazinCanvas != null) magazinCanvas.SetActive(false);
        if (arenaCanvas != null) arenaCanvas.SetActive(false);
        if (evolutionCanvas != null) evolutionCanvas.SetActive(false);
        if (zadanieCanvas != null) zadanieCanvas.SetActive(false);
    }
    
    // --- УПРАВЛЕНИЕ АНИМАЦИЕЙ ---

    /// <summary>
    /// Универсальная функция для запуска последовательности перехода.
    /// Принимает Action (делегат), который будет выполнен, пока экран черный.
    /// </summary>
    public void StartButtonTransition(Action actionToPerform)
    {
        if (faderImage != null)
        {
            // Запускаем корутину, передавая ей, какое действие выполнить.
            StartCoroutine(AnimateIrisTransition(actionToPerform));
        }
        else
        {
            // Если фейдер не найден, выполняем действие немедленно
            actionToPerform?.Invoke(); 
        }
    }

    // Корутина управляет тремя фазами перехода
    private IEnumerator AnimateIrisTransition(Action midpointAction)
    {
        // Включаем фейдер и блокируем взаимодействие
        faderImage.gameObject.SetActive(true);
        CanvasGroup group = faderImage.GetComponent<CanvasGroup>();
        if (group == null) group = faderImage.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = true;
        
        // --- ФАЗА 1: РАСШИРЕНИЕ (ЗАКРЫТИЕ СТАРОГО) ---
        yield return StartCoroutine(ScaleFader(targetScale));
        
        // --- ФАЗА 2: ВЫПОЛНЕНИЕ ДЕЙСТВИЙ И ЗАДЕРЖКА (ЧЕРНЫЙ ЭКРАН) ---
        
        // 1. Выполняем действие, переданное с кнопки (смена Canvas, открытие меню и т.д.)
        midpointAction?.Invoke();
        
        // 2. Ждем заданное время для эффекта "черного экрана"
        if (blackScreenDelay > 0)
        {
            yield return new WaitForSeconds(blackScreenDelay);
        }
        
        // --- ФАЗА 3: СХЛОПЫВАНИЕ (ОТКРЫТИЕ НОВОГО) ---
        yield return StartCoroutine(ScaleFader(0.01f));
        
        // Отключаем фейдер и разблокируем взаимодействие
        faderImage.gameObject.SetActive(false);
        group.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Корутина для плавного изменения масштаба фейдера.
    /// </summary>
    private IEnumerator ScaleFader(float target)
    {
        Vector3 startScale = faderImage.transform.localScale;
        Vector3 endScale = Vector3.one * target;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            faderImage.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        faderImage.transform.localScale = endScale;
    }

    // --- ЛОГИКА ВАЛЮТЫ (Без изменений) ---

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinDisplay();
    }

    public void AddCrystals(int amount)
    {
        currentCrystals += amount;
        UpdatePointDisplay();
    }

    private void UpdateCoinDisplay()
    {
        if (coinCount != null)
        {
            string formattedCoins = currentCoins.ToString("N0", CultureInfo.InvariantCulture);
            coinCount.text = formattedCoins;
        }
    }

    private void UpdatePointDisplay()
    {
        if (pointCount != null)
        {
            pointCount.text = currentCrystals.ToString();
        }
    }

    private void UpdateAllCurrencyDisplay()
    {
        UpdateCoinDisplay();
        UpdatePointDisplay();
    }
}