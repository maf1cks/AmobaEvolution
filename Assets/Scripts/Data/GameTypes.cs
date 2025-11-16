using UnityEngine;
using System.Collections.Generic;
using System;

public class GameTypes : MonoBehaviour
{
    
}

public enum ValutaType
{
    Coins = 0, // золото
    Experience = 1 // опыт
}

public enum CharacterType
{
    Ameba = 0,
    Sworm = 1,
    Rak = 2,
    Dinosawr = 3
}

[Serializable]
public struct CurrencyAmount
{
    public ValutaType Type;
    public long Amount;

    public CurrencyAmount(ValutaType type, long amount)
    {
        Type = type;
        Amount = amount;
    }
}

[Serializable]
public class GameSaveData
{
    public int Level = 1;
    public CharacterType Character = CharacterType.Ameba;
    public List<CurrencyAmount> Currencies = new List<CurrencyAmount>();
}