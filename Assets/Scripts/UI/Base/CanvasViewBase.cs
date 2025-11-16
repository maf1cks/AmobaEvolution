using Data;
using UnityEngine;

namespace UI
{
    public class CanvasViewBase : MonoBehaviour
    {
        [Header("Key and Canvas")]
        [SerializeField] private CanvasViewKey key = CanvasViewKey.None;
        private Canvas _canvas;
    
        public CanvasViewKey Key => key;
        public Canvas Canvas => _canvas;
    
        public CanvasUIManagerBase CanvasParentManager { get; internal set; }
        public bool IsShown { get; private set; }
    
        protected virtual void Awake()
        {
            if (_canvas == null)
            {
                if (TryGetComponent<Canvas>(out var canvas))
                {
                    _canvas = canvas;
                    return;
                }
                Debug.LogError($"In gameobject - {name} has no canvas Component. Please Add!");
            }
        }
    
        public virtual void Show()
        {
            if (_canvas != null) _canvas.enabled = true;
            gameObject.SetActive(true);
            IsShown = true;
        }
    
        public virtual void Hide()
        {
            IsShown = false;
            if (_canvas != null) _canvas.enabled = false;
            gameObject.SetActive(false);
        }
    }
}