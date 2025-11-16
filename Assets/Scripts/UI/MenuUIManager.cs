using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;
using System; // Добавлено для использования делегата Action

public class UIManager : MonoBehaviour
{   
    // --- НОВЫЕ ПОЛЯ ДЛЯ ЭКРАНОВ РЕЗУЛЬТАТА ---
    [Header("Экраны Выигрыша/Проигрыша")]
    // Панель, которая показывается при проигрыше/выигрыше (содержит текст и кнопку)
    [SerializeField] private GameObject resultScreenPanel; 
    // Кнопка, которую игрок должен нажать, чтобы начать анимацию возврата в меню
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI resultText; // Текст для отображения "Победа" / "Проигрыш"

    [Header("Элементы для Игры")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private SpriteRenderer gameTile;
    [SerializeField] private IntroUIManager introUIManager;
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private GameManager gameManager;
    
    // Все Canvas, которые будут активироваться
    [SerializeField] private GameObject casinoCanvas;
    [SerializeField] private GameObject magazinCanvas;
    [SerializeField] private GameObject arenaCanvas;
    [SerializeField] private GameObject evolutionCanvas;
    [SerializeField] private GameObject zadanieCanvas;
    
    // НОВЫЕ ПОЛЯ ДЛЯ ТЕКСТА В МЕНЮ
    [Header("UI Элементы в Меню")]
    [SerializeField] private TextMeshProUGUI menuField1;
    [SerializeField] private TextMeshProUGUI menuField2;
    [SerializeField] private TextMeshProUGUI menuField3;
    
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
    private int currentXP = 0;
    
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
        currentXP = 100;
        
        UpdateAllCurrencyDisplay();
        
        // --- УНИВЕРСАЛЬНАЯ ПОДПИСКА КНОПОК НА ПЕРЕХОД ---
        if (magazinButton != null) magazinButton.onClick.AddListener(() => StartButtonTransition(OpenMagazinCanvas));
        if (arenaButton != null) arenaButton.onClick.AddListener(() => StartButtonTransition(OpenArenaCanvas));
        if (zadanieButton != null) zadanieButton.onClick.AddListener(() => StartButtonTransition(OpenZadanieCanvas));
        if (evolutionButton != null) evolutionButton.onClick.AddListener(() => StartButtonTransition(OpenEvolutionCanvas));
        if (gameButton != null) gameButton.onClick.AddListener(() => StartButtonTransition(OpenGameCanvas)); 
        if (casinoButton != null) casinoButton.onClick.AddListener(() => StartButtonTransition(OpenCasinoCanvas));
        
        // --- ПОДПИСКА КНОПКИ ДЛЯ ПРОДОЛЖЕНИЯ ---
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonPressed);
        
        // Убеждаемся, что фейдер и панель результата изначально скрыты
        if (faderImage != null)
        {
            faderImage.gameObject.SetActive(false);
            faderImage.transform.localScale = Vector3.one * 0.01f;
        }
        if (resultScreenPanel != null)
        {
            resultScreenPanel.SetActive(false);
        }
    }
    
    // --- НОВЫЙ МЕТОД: Обработчик нажатия кнопки "Продолжить" ---
    private void OnContinueButtonPressed()
    {
        if (resultScreenPanel != null)
        {
            // 1. Скрываем панель результата
            resultScreenPanel.SetActive(false);
            
            // 2. Запускаем анимацию перехода, которая вернет нас в меню
            StartButtonTransition(OpenMenuCanvas);
            Debug.Log("Действие: МАГАЗИН - открыт интерфейс магазина.");

        }
    }

    // --- ДЕЙСТВИЯ, ВЫПОЛНЯЕМЫЕ В СЕРЕДИНЕ ПЕРЕХОДА (ОТКРЫТИЕ CANVAS) ---

    private void OpenMagazinCanvas()
    {
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
        evolutionCanvas.GetComponent<EvolutionManager>().UpdateXP();
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
            introUIManager.SetUpSpritesByLevel(playerPrefab.GetComponent<SpriteRenderer>(), introUIManager.level, 3);
            introUIManager.SetUpSpritesByLevel(gameTile, introUIManager.level, 2);
        }

        gameManager.StartGame();
        
        Debug.Log("Действие: GAME - Меню отключено, игровой процесс активирован.");
    }

    // --- ФУНКЦИИ ВЫВОДА РЕЗУЛЬТАТА ---

    public void OpenGameOverScreen(int enemiesKilledInLevel)
    {
        // 1. Скрываем все игровые элементы (игрока, тайлы и т.д.)
        if (playerPrefab != null) playerPrefab.SetActive(false);
        gameTile.sprite = null;
        
        // 2. Показываем панель результата и устанавливаем текст
        if (resultScreenPanel != null)
        {
            if (resultText != null) resultText.text = "ПРОИГРЫШ";
            if (menuField1 != null) menuField1.text = (enemiesKilledInLevel*2).ToString();
            if (menuField2 != null) menuField2.text = (enemiesKilledInLevel).ToString();
            if (menuField3 != null) menuField3.text = (enemiesKilledInLevel).ToString();
            resultScreenPanel.SetActive(true);
            
        }
        
        Debug.Log("Результат: ПРОИГРЫШ. Ожидание нажатия кнопки для продолжения.");
    }

    public void OpenWinScreen(int enemiesKilledInLevel)
    {
        // 1. Скрываем все игровые элементы
        if (playerPrefab != null) playerPrefab.SetActive(false);
        gameTile.sprite = null;
        
        // 2. Показываем панель результата и устанавливаем текст
        if (resultScreenPanel != null)
        {
            if (resultText != null) resultText.text = "ПОБЕДА!";
            if (menuField1 != null) menuField1.text = (enemiesKilledInLevel*2).ToString();
            if (menuField2 != null) menuField2.text = (enemiesKilledInLevel).ToString();
            if (menuField3 != null) menuField3.text = (enemiesKilledInLevel).ToString();
            resultScreenPanel.SetActive(true);
        }
        
        Debug.Log("Результат: ПОБЕДА. Ожидание нажатия кнопки для продолжения.");
    }
    
    /// <summary>
    /// Действие, которое скрывает текущий активный Canvas и показывает главное меню.
    /// </summary>
    public void OpenMenuCanvas()
    {
        // 1. Скрываем все игровые Canvas
        HideAllCanvases();
        gameTile.sprite = null;

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
    /// НОВЫЙ МЕТОД: Включает или отключает видимость трех новых полей для текста.
    /// </summary>
    private void SetMenuTextFieldsActive(bool isActive)
    {
        if (menuField1 != null) menuField1.gameObject.SetActive(isActive);
        if (menuField2 != null) menuField2.gameObject.SetActive(isActive);
        if (menuField3 != null) menuField3.gameObject.SetActive(isActive);
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

    // --- ЛОГИКА ВАЛЮТЫ ---

    public int GetCurrentCoins()
    {
        return currentCoins;
    }
    public int GetCurrentXP()
    {
        return currentXP;
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinDisplay();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        UpdatePointDisplay();
    }

    private void UpdateCoinDisplay()
    {
        if (coinCount != null)
        {
            // N0 - форматирование с разделителем тысяч (например, 10,000)
            string formattedCoins = currentCoins.ToString("N0", CultureInfo.InvariantCulture);
            coinCount.text = formattedCoins;
        }
    }

    private void UpdatePointDisplay()
    {
        if (pointCount != null)
        {
            pointCount.text = currentXP.ToString();
        }
    }

    private void UpdateAllCurrencyDisplay()
    {
        UpdateCoinDisplay();
        UpdatePointDisplay();
    }
}