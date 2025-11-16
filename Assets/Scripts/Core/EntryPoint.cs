using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class EntryPoint : MonoBehaviour
{
    public static EntryPoint Instance { get; private set; }

    [Tooltip("Менеджеры в порядке инициализации (сверху вниз).")]
    [SerializeField] private List<BaseManager> managersInOrder = new List<BaseManager>();

    private readonly Dictionary<Type, BaseManager> _typeMap = new Dictionary<Type, BaseManager>();
    public bool IsInitialized { get; private set; }

    public event Action OnInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[EntryPoint] Дубликат EntryPoint. Уничтожаю компонент.", this);
            Destroy(this);
            return;
        }
        Instance = this;

        // Если список пуст — попробуем авто-собрать компоненты с этого объекта (порядок = порядок в инспекторе)
        if (managersInOrder.Count == 0)
        {
            var found = GetComponents<BaseManager>();
            managersInOrder.AddRange(found);
        }

        // Ссылка на родителя + первичная регистрация
        for (int i = 0; i < managersInOrder.Count; i++)
        {
            var m = managersInOrder[i];
            if (m == null) continue;

            // Проставляем EntryPoint
            m.ParentEntryPoint = this;

            var t = m.GetType();
            if (_typeMap.ContainsKey(t))
            {
                Debug.LogWarning($"[EntryPoint] Менеджер типа {t.Name} уже зарегистрирован. Дубликат будет проигнорирован.", m);
                continue;
            }
            _typeMap.Add(t, m);
        }
    }

    private void Start()
    {
        // Последовательно запускаем менеджеры в порядке списка
        foreach (var m in managersInOrder)
        {
            if (m == null) continue;
#if UNITY_EDITOR
            // Для наглядности
            // Debug.Log($"[EntryPoint] Setup -> {m.GetType().Name}");
#endif
            m.Setup();
            m.IsSetup = true;
        }

        IsInitialized = true;
        OnInitialized?.Invoke();
    }

    private void OnDestroy()
    {
        // Dispose в обратном порядке
        for (int i = managersInOrder.Count - 1; i >= 0; i--)
        {
            var m = managersInOrder[i];
            if (m == null) continue;
            m.Dispose();
        }

        if (Instance == this) Instance = null;
    }

    // Получение менеджера по типу
    public T GetManager<T>() where T : BaseManager
    {
        // Ищем точное совпадение типа
        if (_typeMap.TryGetValue(typeof(T), out var exact))
            return exact as T;

        // Ищем совместимый тип (на случай наследников)
        foreach (var kv in _typeMap)
        {
            if (kv.Value is T hit) return hit;
        }
        return null;
    }
}