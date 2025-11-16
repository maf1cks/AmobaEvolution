using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderbordDataSO", menuName = "Configs/Leaderbord Data", order = 0)]
public class LeaderbordDataSO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string Name;
        public int Kills;
        public Sprite Icon; // если null — будет дефолтная иконка из GameAssetsDatabase
    }

    public List<Entry> Entries = new List<Entry>();
}