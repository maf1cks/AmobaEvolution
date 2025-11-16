using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : BaseManager
{
    private const string SaveKey = "player_save_v1";

    public GameSaveData LoadedData { get; private set; }

    public override void Setup()
    {
        base.Setup();
        LoadedData = LoadFromPrefsOrDefault();
        EnsureAllCurrenciesPresent(LoadedData);
#if UNITY_EDITOR
        // Debug.Log($"[SaveDataManager] Loaded: {JsonUtility.ToJson(LoadedData)}");
#endif
    }

    public void SaveAll()
    {
        var player = GetManager<PlayerDataManager>();
        var valuta = GetManager<ValutaManager>();

        if (player == null || valuta == null)
        {
            Debug.LogError("[SaveDataManager] Менеджеры не готовы для сохранения.");
            return;
        }

        var data = new GameSaveData
        {
            Level = player.Level,
            Character = player.Character,
            Currencies = valuta.GetAllForSave()
        };

        SaveToPrefs(data);
        LoadedData = data;
    }

    public void SaveManual(GameSaveData data)
    {
        EnsureAllCurrenciesPresent(data);
        SaveToPrefs(data);
        LoadedData = data;
    }

    private GameSaveData LoadFromPrefsOrDefault()
    {
        var json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            // Новая игра
            return new GameSaveData
            {
                Level = 1,
                Character = CharacterType.Ameba,
                Currencies = new List<CurrencyAmount>
                {
                    new CurrencyAmount(ValutaType.Coins, 5),
                    new CurrencyAmount(ValutaType.Experience, 0)
                }
            };
        }
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    private void SaveToPrefs(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
#if UNITY_EDITOR
        // Debug.Log($"[SaveDataManager] Saved: {json}");
#endif
    }

    private void EnsureAllCurrenciesPresent(GameSaveData data)
    {
        var present = new HashSet<ValutaType>();
        foreach (var c in data.Currencies)
            present.Add(c.Type);

        foreach (ValutaType type in System.Enum.GetValues(typeof(ValutaType)))
        {
            if (!present.Contains(type))
                data.Currencies.Add(new CurrencyAmount(type, 0));
        }
    }

    private void OnApplicationQuit()
    {
        // Безопасное сохранение на выходе
        SaveAll();
    }
}