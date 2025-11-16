using TMPro;
using UnityEngine;
using UnityEngine.UI;
    
namespace UI.Runtime.View
{
    public class RuntimeLeaderbordLoadingView : MonoBehaviour
    {
        private Image _overlay;
        private TextMeshProUGUI _text;
    
        public void Build(TMP_FontAsset font, Color overlayColor, string message, int fontSize = 22)
        {
            // Растягиваем на всю доску
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
    
            // Тёмный полупрозрачный фон
            _overlay = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            _overlay.color = overlayColor;        // например, new Color(0,0,0,0.5f)
            _overlay.raycastTarget = true;        // блокирует клики скролла под собой
    
            // Текст по центру
            var textGO = new GameObject("LoadingText", typeof(RectTransform));
            textGO.transform.SetParent(transform, false);
            var tr = textGO.GetComponent<RectTransform>();
            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
    
            _text = textGO.AddComponent<TextMeshProUGUI>();
            _text.font = font != null ? font : TMP_Settings.defaultFontAsset;
            _text.fontSize = fontSize;
            _text.text = string.IsNullOrEmpty(message) ? "Загрузка..." : message;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = Color.white;
        }
    
        public void Show(bool visible)
        {
            gameObject.SetActive(visible);
        }
    
        public void SetText(string text)
        {
            if (_text != null) _text.text = text;
        }
    
        public void SetColor(Color color)
        {
            if (_overlay != null) _overlay.color = color;
        }
    }
}