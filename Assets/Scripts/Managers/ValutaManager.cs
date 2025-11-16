using System;
using System.Collections.Generic;
using UnityEngine;

public class ValutaManager : BaseManager
{
    public event Action<ValutaType> AddValutaEvent;

    private readonly Dictionary<ValutaType, long> _wallet = new Dictionary<ValutaType, long>();

    public override void Setup()
    {
        base.Setup();

        // Гарантируем наличие всех типов
        foreach (ValutaType type in Enum.GetValues(typeof(ValutaType)))
            if (!_wallet.ContainsKey(type))
                _wallet[type] = 0;

        // Применяем данные из сейва (если есть)
        var save = GetManager<SaveDataManager>()?.LoadedData;
        if (save != null)
            SetAllFromSave(save.Currencies, notify: true);
    }

    public void AddValuta(ValutaType type, long amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[ValutaManager] Попытка добавить некорректное количество: {amount} для {type}");
            return;
        }
        _wallet[type] = GetAmount(type) + amount;
        AddValutaEvent?.Invoke(type);
    }

    public bool TrySpend(ValutaType type, long amount)
    {
        if (amount <= 0) return true;

        long current = GetAmount(type);
        if (current < amount) return false;

        _wallet[type] = current - amount;
        AddValutaEvent?.Invoke(type);
        return true;
    }

    public long GetAmount(ValutaType type)
    {
        return _wallet.TryGetValue(type, out var value) ? value : 0;
    }

    public List<CurrencyAmount> GetAllForSave()
    {
        var list = new List<CurrencyAmount>();
        foreach (var kv in _wallet)
            list.Add(new CurrencyAmount(kv.Key, kv.Value));
        return list;
    }

    public void SetAllFromSave(List<CurrencyAmount> currencies, bool notify = true)
    {
        _wallet.Clear();

        if (currencies != null)
        {
            foreach (var c in currencies)
                _wallet[c.Type] = c.Amount;
        }

        foreach (ValutaType type in Enum.GetValues(typeof(ValutaType)))
            if (!_wallet.ContainsKey(type))
                _wallet[type] = 0;

        if (notify && AddValutaEvent != null)
        {
            foreach (ValutaType type in Enum.GetValues(typeof(ValutaType)))
                AddValutaEvent.Invoke(type);
        }
    }
}