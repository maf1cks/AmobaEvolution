using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Data;
using UI;

public class CasinoCanvas : CanvasViewBase
{
    [Header("Ссылки на UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private TextMeshProUGUI winText;
    private MenuCanvas menuCanvas;
    
    [Header("Настройки колеса")]
    // Компонент Transform колеса, который будет вращаться
    [SerializeField] private Transform wheelCasinoTransform; 
    
    // Новые поля для случайной начальной скорости
    [SerializeField] private float minInitialSpeed = 800f; // Минимальная начальная скорость вращения
    [SerializeField] private float maxInitialSpeed = 1200f; // Максимальная начальная скорость вращения
    
    [SerializeField] private float decelerationRate = 50f; // Скорость замедления
    [SerializeField] private float minSpinTime = 2f; // Минимальное время вращения (фаза быстрой скорости)
    [SerializeField] private float maxSpinTime = 5f; // Максимальное время вращения (фаза быстрой скорости)

    private bool isSpinning = false;
    private float currentSpeed;

    private void Start()
    {
        if (menuCanvas == null)
        {
            // Поиск UIManager, если не задан в Инспекторе
            menuCanvas = CanvasParentManager.TryGetCanvasByType<MenuCanvas>();
            if (menuCanvas == null)
            {
                Debug.LogError("CasinoManager не нашел UIManager в сцене. Обратная навигация невозможна.");
            }
        }
        
        // Подписка кнопки "Назад" на переход в главное меню через UIManager
        if (backButton != null && menuCanvas != null)
        {
            backButton.onClick.AddListener(() =>
            {
                // Запускаем переход
                CanvasParentManager.SetView(CanvasViewKey.Menu);
            });
        }

        // Подписка кнопки "Крутить"
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(StartSpin);
        }

        // Изначально скрываем сообщение о выигрыше
        winText.text = "";
    }

    /// <summary>
    /// Запускает процесс вращения колеса со случайной начальной скоростью.
    /// </summary>
    private void StartSpin()
    {
        if (isSpinning) return;
        
        isSpinning = true;
        spinButton.interactable = false; // Блокируем кнопку на время вращения
        winText.text = "Крутится...";

        // !!! Задаем СЛУЧАЙНУЮ начальную скорость
        currentSpeed = UnityEngine.Random.Range(minInitialSpeed, maxInitialSpeed);
        StartCoroutine(SpinWheel());
    }

    /// <summary>
    /// Корутина для вращения и замедления колеса.
    /// </summary>
    private IEnumerator SpinWheel()
    {
        // Время, пока колесо вращается с высокой скоростью (до начала замедления)
        float fastSpinDuration = UnityEngine.Random.Range(minSpinTime, maxSpinTime);
        float elapsedTime = 0f;

        // --- Фаза быстрого вращения ---
        while (elapsedTime < fastSpinDuration)
        {
            // Вращаем колесо по оси Z
            wheelCasinoTransform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // --- Фаза замедления и остановки ---
        while (currentSpeed > 0)
        {
            // Вращаем колесо
            wheelCasinoTransform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);
            
            // Постепенно уменьшаем скорость (линейное замедление)
            currentSpeed -= decelerationRate * Time.deltaTime;
            
            if (currentSpeed < 0) currentSpeed = 0;

            yield return null;
        }

        // Колесо остановилось
        isSpinning = false;
        spinButton.interactable = true;
        DetermineWin();
    }

    /// <summary>
    /// Определяет результат после остановки колеса по его финальному углу вращения.
    /// Мы предполагаем 12 секторов по 30 градусов каждый.
    /// </summary>
    private void DetermineWin()
    {
        // Получаем угол Z в локальных координатах. 
        // Приводим его к диапазону [0, 360) для удобства сравнения.
        float finalAngle = wheelCasinoTransform.localEulerAngles.z % 360;
        if (finalAngle < 0) finalAngle += 360; // Убедимся, что угол положительный
        
        string winMessage;
        
        // Ширина одного сектора
        const float sectorAngle = 30f; 

        // Определяем сектор (диапазоны: [0-30), [30-60), [60-90), ..., [330-360) )

        if (finalAngle >= 0 * sectorAngle && finalAngle < 1 * sectorAngle) // Сектор 1 (0° - 30°)
        {
            winMessage = "Вы выиграли 50 монет!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 50);
        }
        else if (finalAngle >= 1 * sectorAngle && finalAngle < 2 * sectorAngle) // Сектор 2 (30° - 60°)
        {
            winMessage = "Вы выиграли 1 кристалл!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Experience, 1);
        }
        else if (finalAngle >= 2 * sectorAngle && finalAngle < 3 * sectorAngle) // Сектор 3 (60° - 90°)
        {
            winMessage = "Вы выиграли 100 монет!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 100);
        }
        else if (finalAngle >= 3 * sectorAngle && finalAngle < 4 * sectorAngle) // Сектор 4 (90° - 120°)
        {
            winMessage = "Не повезло! Попробуйте снова.";
        }
        else if (finalAngle >= 4 * sectorAngle && finalAngle < 5 * sectorAngle) // Сектор 5 (120° - 150°)
        {
            winMessage = "Вы выиграли 150 монет!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 150);
        }
        else if (finalAngle >= 5 * sectorAngle && finalAngle < 6 * sectorAngle) // Сектор 6 (150° - 180°)
        {
            winMessage = "Вау! Вы выиграли 5 кристаллов!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 150);
        }
        else if (finalAngle >= 6 * sectorAngle && finalAngle < 7 * sectorAngle) // Сектор 7 (180° - 210°)
        {
            winMessage = "Вы выиграли 25 монет!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 150);
        }
        else if (finalAngle >= 7 * sectorAngle && finalAngle < 8 * sectorAngle) // Сектор 8 (210° - 240°)
        {
            winMessage = "Вы выиграли 2 кристалла!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Experience, 2);
        }
        else if (finalAngle >= 8 * sectorAngle && finalAngle < 9 * sectorAngle) // Сектор 9 (240° - 270°)
        {
            winMessage = "Вы выиграли 300 монет!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 300);
        }
        else if (finalAngle >= 9 * sectorAngle && finalAngle < 10 * sectorAngle) // Сектор 10 (270° - 300°)
        {
            winMessage = "Не повезло! Попробуйте снова.";
        }
        else if (finalAngle >= 10 * sectorAngle && finalAngle < 11 * sectorAngle) // Сектор 11 (300° - 330°)
        {
            winMessage = "Джекпот! Вы выиграли 500 монет!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 500);
        }
        else // Сектор 12 (330° - 360°)
        {
            winMessage = "Вы выиграли 1 кристалл!";
            EntryPoint.Instance.GetManager<ValutaManager>().AddValuta(ValutaType.Coins, 1);
        }
        
        winText.text = winMessage;
        // F2 форматирует число с двумя знаками после запятой
        Debug.Log($"Казино: Колесо остановилось на угле {finalAngle:F2}°. 12 секторов по 30°. Результат: {winMessage}");
    }
}