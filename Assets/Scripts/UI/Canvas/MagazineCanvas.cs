using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MagazineCanvas : CanvasViewBase
    {
        [SerializeField] public Button closeButton;

        public void Start()
        {
            // Подписка кнопки "Назад" на переход в главное меню через UIManager
            if (closeButton != null && CanvasParentManager != null)
            {
                // Запускаем переход, передавая действие OpenMenuCanvas
                closeButton.onClick.AddListener(() =>
                {
                    CanvasParentManager.SetView(CanvasViewKey.Menu);
                });
            }
        }
    }
}