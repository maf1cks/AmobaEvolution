using UnityEngine;

public class LoadAssetsManager : BaseManager
{
    [SerializeField] private string resourcesPath = "GameAssetsDatabase";
    [SerializeField] private GameAssetsDatabase database;

    public override void Setup()
    {
        base.Setup();

        if (database == null)
        {
            database = Resources.Load<GameAssetsDatabase>(resourcesPath);
            if (database == null)
            {
                Debug.LogError($"[LoadAssetsManager] Не найден GameAssetsDatabase в Resources по пути: {resourcesPath}");
            }
        }
    }

    public Sprite GetValutaSprite(ValutaType type)
    {
        return database ? database.GetCurrencyIcon(type) : null;
    }

    // Лидерборд: ассеты по умолчанию
    public Sprite GetLeaderbordBackground() => database ? database.LeaderbordBackground : null;
    public Sprite GetLeaderbordKillsIcon() => database ? database.LeaderbordKillsIcon : null;
}