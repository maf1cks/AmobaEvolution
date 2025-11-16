using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAssetsDatabase", menuName = "Configs/Game Assets Database", order = 0)]
public class GameAssetsDatabase : ScriptableObject
{
    [Serializable]
    public class ValutaSpriteEntry
    {
        public ValutaType Type;
        public Sprite Icon;
    }

    [Header("Иконки валют")]
    public List<ValutaSpriteEntry> CurrencyIcons = new List<ValutaSpriteEntry>();

    [Header("Лидерборд")]
    public Sprite LeaderbordBackground;    // фон (tile/tiled)
    public Sprite LeaderbordKillsIcon;     // иконка убийств по умолчанию

    private Dictionary<ValutaType, Sprite> _currencyMap;

    private void OnEnable() => RebuildMaps();
    private void OnValidate() => RebuildMaps();

    private void RebuildMaps()
    {
        _currencyMap = new Dictionary<ValutaType, Sprite>();
        foreach (var entry in CurrencyIcons)
        {
            if (entry == null) continue;
            _currencyMap[entry.Type] = entry.Icon;
        }
    }

    public Sprite GetCurrencyIcon(ValutaType type)
    {
        if (_currencyMap != null && _currencyMap.TryGetValue(type, out var sprite))
            return sprite;
        return null;
    }
}