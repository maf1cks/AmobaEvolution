using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Globalization; // Добавлено для форматирования

/// <summary>
/// Управляет логикой колеса фортуны: вращением, стоимостью, определением выигрыша.
/// </summary>
public class CasinoManager : MonoBehaviour
{
    [Header("Ссылки на UI и Менеджеры")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Button backButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private TextMeshProUGUI winText;
    
    [Header("Настройки колеса")]
    [Tooltip("Компонент Transform колеса, который будет вращаться.")]
    [SerializeField] private Transform wheelCasinoTransform; 
    
    [Tooltip("Минимальная начальная скорость вращения (градусы/сек).")]
    [SerializeField] private float minInitialSpeed = 800f; 
    [Tooltip("Максимальная начальная скорость вращения (градусы/сек).")]
    [SerializeField] private float maxInitialSpeed = 1000f; 
    
    [Tooltip("Скорость замедления (градусы/сек^2).")]
    [SerializeField] private float decelerationRate = 900f; 
    [Tooltip("Минимальное время вращения с высокой скоростью.")]
    [SerializeField] private float minSpinTime = 2f; 
    [Tooltip("Максимальное время вращения с высокой скоростью.")]
    [SerializeField] private float maxSpinTime = 5f; 

    [Header("Настройки отображения текста")]
    [Tooltip("Время, через которое сообщение о выигрыше исчезнет.")]
    [SerializeField] private float winMessageDuration = 3.0f; 

    // --- Логика стоимости крутки ---
    private bool isFirstSpinOfDay = true; // Флаг для первой бесплатной крутки
    private const int SPIN_COST = 50; // Стоимость последующих круток
    private const float SECTOR_ANGLE = 30f; // Ширина одного сектора (360/12)

    private bool isSpinning = false;
    private float currentSpeed;

    private void Start()
    {
        // Инициализация ссылок на UIManager, если они не заданы
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("CasinoManager не нашел UIManager в сцене. Управление валютой и навигация будут недоступны.");
            }
        }
        
        // Подписка кнопки "Назад" на переход в главное меню через UIManager
        if (backButton != null && uiManager != null)
        {
            backButton.onClick.AddListener(() => uiManager.StartButtonTransition(uiManager.OpenMenuCanvas));
            backButton.interactable = !isSpinning;
        }

        // Подписка кнопки "Крутить"
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(StartSpin);
        }

        // Изначально скрываем сообщение о выигрыше
        winText.text = "";
        
        Debug.Log($"Состояние казино: Первая крутка сегодня {(isFirstSpinOfDay ? "бесплатна" : $"стоит {SPIN_COST} монет")}.");
    }

    /// <summary>
    /// Внешняя функция для сброса ежедневной бесплатной крутки (для использования, например, при смене дня).
    /// </summary>
    public void ResetDailySpin()
    {
        isFirstSpinOfDay = true;
        Debug.Log("Ежедневная бесплатная крутка была сброшена.");
    }

    /// <summary>
    /// Запускает процесс вращения колеса, включая проверку стоимости.
    /// </summary>
    private void StartSpin()
    {
        if (isSpinning || uiManager == null) return;
        
        // Останавливаем любую корутину очистки текста, чтобы новое сообщение не исчезло сразу
        StopAllCoroutines(); 
        
        string costMessage = "";
        bool canSpin = false;
        
        // --- ЛОГИКА СТОИМОСТИ КРУТКИ ---
        if (isFirstSpinOfDay)
        {
            // БЕСПЛАТНАЯ КРУТКА
            isFirstSpinOfDay = false; 
            costMessage = "<size=120%><color=#33FF33>Крутка БЕСПЛАТНА!</color></size>";
            canSpin = true;
        }
        else
        {
            // ПЛАТНАЯ КРУТКА
            if (uiManager.GetCurrentCoins() >= SPIN_COST)
            {
                uiManager.AddCoins(-SPIN_COST); // Списываем монеты
                costMessage = $"<size=120%><color=#FFFFFF>Списано {SPIN_COST} монет. Удачи!</color></size>";
                canSpin = true;
            }
            else
            {
                // Недостаточно монет
                costMessage = $"<size=150%><b><color=#FF4500>НЕДОСТАТОЧНО МОНЕТ (требуется {SPIN_COST})</color></b></size>";
                winText.text = costMessage;
                // Показываем сообщение о нехватке монет дольше, чем обычный выигрыш
                StartCoroutine(ClearWinTextAfterDelay(winMessageDuration * 2)); 
                Debug.LogWarning("Попытка крутки: Недостаточно монет.");
                return; 
            }
        }
        
        // --- ЗАПУСК ВРАЩЕНИЯ ---
        if (!canSpin) return;

        isSpinning = true;
        spinButton.interactable = false; 
        
        if (backButton != null)
        {
             backButton.interactable = false; // Блокируем кнопку "Назад"
        }
        
        // Показываем сообщение о стоимости перед началом вращения
        winText.text = costMessage;

        // Задаем СЛУЧАЙНУЮ начальную скорость
        currentSpeed = UnityEngine.Random.Range(minInitialSpeed, maxInitialSpeed);
        
        StartCoroutine(SpinWheel());
    }

    /// <summary>
    /// Корутина для вращения и замедления колеса.
    /// </summary>
    private IEnumerator SpinWheel()
    {
        float fastSpinDuration = UnityEngine.Random.Range(minSpinTime, maxSpinTime);
        float elapsedTime = 0f;

        // --- Фаза 1: Быстрое вращение ---
        while (elapsedTime < fastSpinDuration)
        {
            wheelCasinoTransform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Очищаем winText перед началом замедления
        winText.text = "";

        // --- Фаза 2: Замедление и остановка ---
        while (currentSpeed > 0)
        {
            wheelCasinoTransform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);
            
            currentSpeed -= decelerationRate * Time.deltaTime;
            
            if (currentSpeed < 0) currentSpeed = 0;

            yield return null;
        }

        // --- Остановка ---
        isSpinning = false;
        spinButton.interactable = true;
        
        if (backButton != null)
        {
             backButton.interactable = true; // Разблокируем кнопку "Назад"
        }
        
        DetermineWin();
    }

    /// <summary>
    /// Определяет результат после остановки колеса по его финальному углу вращения.
    /// Предполагается 12 секторов по 30 градусов каждый.
    /// </summary>
    private void DetermineWin()
    {
        // Получаем угол Z в локальных координатах и приводим его к диапазону [0, 360)
        float finalAngle = wheelCasinoTransform.localEulerAngles.z % 360f;
        if (finalAngle < 0f) finalAngle += 360f;
        
        // Смещаем угол, чтобы центр сектора соответствовал оси вращения (середина сектора 30/2 = 15 градусов)
        float shiftedAngle = (finalAngle + SECTOR_ANGLE / 2f) % 360f;

        // Определяем 0-базовый индекс сектора (0 до 11)
        int sectorIndex = Mathf.FloorToInt(shiftedAngle / SECTOR_ANGLE);
        // Фактический номер сектора (1 до 12)
        int sectorNumber = sectorIndex + 1;
        
        string winMessage;

        // Определяем стиль для выигрыша: Большой, жирный, золотой
        string tagStart = "<size=150%><b><color=#FFD700>"; 
        string tagEnd = "</color></b></size>";

        // Используем switch по номеру сектора
        switch (sectorNumber)
        {
            case 1: 
                winMessage = "Вы выиграли 100 монет!";
                uiManager?.AddCoins(100);
                break;
            case 2: 
                winMessage = "Вы выиграли удвоитель опыта!";
                break;
            case 3: 
                winMessage = "Вы выиграли 40 монет!";
                uiManager?.AddCoins(40);
                break;
            case 4: 
                winMessage = "Вы выиграли удвоитель монет!";
                break;
            case 5: 
                winMessage = "Вы выиграли удвоитель скорости!";
                break;
            case 6: 
                winMessage = "Вы выиграли 40 монет!";
                uiManager?.AddCoins(40);
                break;
            case 7: 
                winMessage = "Вы выиграли 50 опыта!";
                uiManager?.AddXP(50);
                break;
            case 8: 
                winMessage = "Вы выиграли 100 монет!";
                uiManager?.AddCoins(100);
                break;
            case 9: 
                winMessage = "Вы выиграли 40 монет!";
                uiManager?.AddCoins(40);
                break;
            case 10: 
                winMessage = "Вы выиграли удвоитель здоровья!";
                break;
            case 11: 
                winMessage = "ДЖЕКПОТ! Вы выиграли 40 монет!";
                uiManager?.AddCoins(40);
                break;
            case 12: 
                winMessage = "Вы выиграли удвоитель урона!";
                break;
            default:
                // При ошибке форматирование сбрасывается для более строгого сообщения
                tagStart = "<color=#FF4500>"; 
                tagEnd = "</color>";
                winMessage = "Ошибка определения сектора. Угол: " + finalAngle.ToString("F2", CultureInfo.InvariantCulture);
                Debug.LogError($"Не удалось определить сектор выигрыша. Сдвинутый угол: {shiftedAngle:F2}, Индекс: {sectorIndex}");
                break;
        }
        
        // Применяем форматирование к сообщению о выигрыше
        winText.text = tagStart + winMessage + tagEnd;
        
        StartCoroutine(ClearWinTextAfterDelay(winMessageDuration));

        Debug.Log($"Казино: Колесо остановилось на угле {finalAngle:F2}° (Сектор {sectorNumber}). Результат: {winMessage}");
    }

    /// <summary>
    /// Корутина, которая очищает текст о выигрыше после заданной задержки.
    /// </summary>
    private IEnumerator ClearWinTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        winText.text = "";
    }
}