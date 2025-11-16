using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuntimeLeaderbordScoreView : MonoBehaviour
{
    public TextMeshProUGUI NameText { get; private set; }
    public Image IconImage { get; private set; }
    public TextMeshProUGUI KillsText { get; private set; }

    private RectTransform _rightGroupRT;

    public void Build(TMP_FontAsset fontAsset, int height, int leftPadding, int rightPadding, int fontSize = 18, int iconSize = 24, int iconTextSpacing = 6)
    {
        // Корневой контейнер строки
        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0, height);

        var layoutElement = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;

        var rootLayout = GetComponent<HorizontalLayoutGroup>() ?? gameObject.AddComponent<HorizontalLayoutGroup>();
        rootLayout.spacing = 8;
        rootLayout.padding = new RectOffset(leftPadding, rightPadding, 0, 0);
        rootLayout.childAlignment = TextAnchor.MiddleCenter;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandHeight = true;

        // Важно: пусть лейаут контролирует ширину детей, но не форсит растяжение
        rootLayout.childControlWidth = true;
        rootLayout.childForceExpandWidth = false;

        // Имя слева
        var nameGO = new GameObject("NameText", typeof(RectTransform));
        nameGO.transform.SetParent(transform, false);
        NameText = nameGO.AddComponent<TextMeshProUGUI>();
        NameText.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        NameText.fontSize = fontSize;
        NameText.color = Color.white;
        NameText.alignment = TextAlignmentOptions.MidlineLeft;
        NameText.enableWordWrapping = false;
        NameText.overflowMode = TextOverflowModes.Ellipsis;

        // Не даём имени забирать гибкую ширину (для этого есть Spacer)
        var nameLE = nameGO.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 0; // предпочитаем ширину по содержимому

        // Spacer — забирает всё свободное пространство
        var spacerGO = new GameObject("Spacer", typeof(RectTransform));
        spacerGO.transform.SetParent(transform, false);
        var spacerLE = spacerGO.AddComponent<LayoutElement>();
        spacerLE.flexibleWidth = 1;
        spacerLE.minWidth = 0;

        // Правая группа (иконка + текст киллов)
        var rightGroupGO = new GameObject("RightGroup", typeof(RectTransform));
        rightGroupGO.transform.SetParent(transform, false);
        _rightGroupRT = rightGroupGO.GetComponent<RectTransform>();

        var rightLayout = rightGroupGO.AddComponent<HorizontalLayoutGroup>();
        rightLayout.spacing = iconTextSpacing;
        rightLayout.childAlignment = TextAnchor.MiddleRight;

        // Важно: контролируем ширину детей по их preferred, но не растягиваем
        rightLayout.childControlWidth = true;
        rightLayout.childForceExpandWidth = false;

        rightLayout.childControlHeight = true;
        rightLayout.childForceExpandHeight = false;

        // Фиксируем ширину иконки через LayoutElement (иначе LayoutGroup может переопределить sizeDelta)
        var iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(rightGroupGO.transform, false);
        IconImage = iconGO.AddComponent<Image>();
        IconImage.preserveAspect = true;

        var iconLE = iconGO.AddComponent<LayoutElement>();
        iconLE.preferredWidth = iconSize;
        iconLE.preferredHeight = iconSize;
        iconLE.minWidth = iconSize;
        iconLE.minHeight = iconSize;

        // Текст киллов — ширина по содержимому (TMP сам отдаёт preferredWidth)
        var killsGO = new GameObject("KillsText", typeof(RectTransform));
        killsGO.transform.SetParent(rightGroupGO.transform, false);
        KillsText = killsGO.AddComponent<TextMeshProUGUI>();
        KillsText.font = NameText.font;
        KillsText.fontSize = fontSize;
        KillsText.color = Color.white;
        KillsText.alignment = TextAlignmentOptions.MidlineRight;
        KillsText.enableWordWrapping = false;
        KillsText.overflowMode = TextOverflowModes.Overflow;

        // Говорим лейауту, что этот элемент не гибкий, берёт столько, сколько нужно
        var killsLE = killsGO.AddComponent<LayoutElement>();
        killsLE.flexibleWidth = 0; // ширина = preferredWidth текста
        killsLE.minWidth = 0;
    }

    public void SetData(string name, int kills, Sprite icon)
    {
        if (NameText) NameText.text = name ?? string.Empty;
        if (KillsText) KillsText.text = kills.ToString();
        if (IconImage)
        {
            IconImage.sprite = icon;
            IconImage.enabled = icon != null;
        }

        // Пересобрать лейаут, чтобы preferredWidth текста учёл новое значение
        if (_rightGroupRT != null)
            LayoutRebuilder.MarkLayoutForRebuild(_rightGroupRT);
        var myRT = GetComponent<RectTransform>();
        if (myRT != null)
            LayoutRebuilder.MarkLayoutForRebuild(myRT);
    }

    // Опционально: менять размер иконки в рантайме
    public void SetIconSize(int newSize)
    {
        if (IconImage == null) return;
        var le = IconImage.GetComponent<LayoutElement>();
        if (le != null)
        {
            le.preferredWidth = newSize;
            le.preferredHeight = newSize;
            le.minWidth = newSize;
            le.minHeight = newSize;
            if (_rightGroupRT != null)
                LayoutRebuilder.MarkLayoutForRebuild(_rightGroupRT);
        }
    }
}