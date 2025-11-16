using UnityEngine;

public class PlayerDataManager : BaseManager
{
    [SerializeField] private int level = 1;
    [SerializeField] private CharacterType character = CharacterType.Ameba;

    public int Level => level;
    public CharacterType Character => character;

    public override void Setup()
    {
        base.Setup();

        // Применяем данные из сейва
        var save = GetManager<SaveDataManager>()?.LoadedData;
        if (save != null)
        {
            SetLevel(save.Level);
            SetCharacter(save.Character);
        }
    }

    public void SetLevel(int newLevel)
    {
        if (newLevel < 1) newLevel = 1;
        level = newLevel;
    }

    public void SetCharacter(CharacterType newCharacter)
    {
        character = newCharacter;
    }
}