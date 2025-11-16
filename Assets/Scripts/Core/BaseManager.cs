using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    public EntryPoint ParentEntryPoint { get; internal set; }
    public bool IsSetup { get; internal set; }

    // Вызывается EntryPoint в фиксированном порядке
    public virtual void Setup() { IsSetup = true; }

    // Вызывается EntryPoint при выгрузке
    public virtual void Dispose() { }

    // Удобный доступ к другим менеджерам
    protected T GetManager<T>() where T : BaseManager
    {
        return ParentEntryPoint ? ParentEntryPoint.GetManager<T>() : null;
    }
}