using TMPro;
using UI.Runtime.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RuntimeLeaderbord : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 size = new Vector2(450, 500);
    [SerializeField] private int headerTopOffset = 110;
    [SerializeField] private int contentSpacing = 6;
    [SerializeField] private int itemHeight = 35;
    [SerializeField] private int itemLeftPadding = 30;
    [SerializeField] private int itemRightPadding = 30;
    [SerializeField] private int fontSize = 18;

    [Header("Visuals")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Color backgroundColor = new Color(0.85f, 0.85f, 0.85f);

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset fallbackTMPFont;

    [Header("Loading Overlay")]
    [SerializeField] private string loadingMessage = "Загрузка...";
    [SerializeField] private Color loadingOverlayColor = new Color(0f, 0f, 0f, 0.5f);

    public RectTransform ContentRect { get; private set; }
    public ScrollRect ScrollRect { get; private set; }
    public RuntimeLeaderbordLoadingView LoadingView { get; private set; }

    private TMP_FontAsset _resolvedTMPFont;
    private Coroutine _loadingRoutine;

    public static RuntimeLeaderbord Create(Canvas parentCanvas, Sprite bgSprite, Vector2? overrideSize = null, int headerOffset = 100)
    {
        EnsureEventSystem();

        var go = new GameObject("RuntimeLeaderbord", typeof(RectTransform));
        go.transform.SetParent(parentCanvas.transform, false);

        var lb = go.AddComponent<RuntimeLeaderbord>();
        lb.backgroundSprite = bgSprite;
        if (overrideSize.HasValue) lb.size = overrideSize.Value;
        lb.headerTopOffset = headerOffset;
        lb.Build();
        return lb;
    }

    public void Build()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        // Фон
        var bg = gameObject.AddComponent<Image>();
        bg.color = backgroundColor;
        bg.sprite = backgroundSprite;
        bg.type = Image.Type.Sliced;

        // ScrollRect
        ScrollRect = gameObject.AddComponent<ScrollRect>();
        ScrollRect.horizontal = false;
        ScrollRect.vertical = true;
        ScrollRect.movementType = ScrollRect.MovementType.Elastic;
        ScrollRect.scrollSensitivity = 20;

        // Viewport
        var viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(transform, false);
        var viewportRT = viewportGO.GetComponent<RectTransform>();
        viewportRT.anchorMin = new Vector2(0, 0);
        viewportRT.anchorMax = new Vector2(1, 1);
        viewportRT.pivot = new Vector2(0.5f, 0.5f);
        viewportRT.offsetMin = new Vector2(0, 0);
        viewportRT.offsetMax = new Vector2(0, -headerTopOffset);
        
        var rectMask2D = viewportGO.AddComponent<RectMask2D>();
        rectMask2D.padding = new Vector4(0, 20, 0, 0); // ваш padding

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        ContentRect = contentGO.GetComponent<RectTransform>();
        ContentRect.anchorMin = new Vector2(0, 1);
        ContentRect.anchorMax = new Vector2(1, 1);
        ContentRect.pivot = new Vector2(0.5f, 1);
        ContentRect.anchoredPosition = Vector2.zero;
        ContentRect.sizeDelta = new Vector2(0, 0);

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = contentSpacing;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect.viewport = viewportRT;
        ScrollRect.content = ContentRect;

        // TMP font
        _resolvedTMPFont = fallbackTMPFont != null ? fallbackTMPFont : TMP_Settings.defaultFontAsset;
        if (_resolvedTMPFont == null)
        {
            Debug.LogWarning("[RuntimeLeaderbord] TMP default font is null. Import TMP Essential Resources or set fallbackTMPFont.");
        }

        // Loading overlay (поверх всего)
        var loadingGO = new GameObject("LoadingOverlay", typeof(RectTransform));
        loadingGO.transform.SetParent(transform, false);
        LoadingView = loadingGO.AddComponent<RuntimeLeaderbordLoadingView>();
        LoadingView.Build(_resolvedTMPFont, loadingOverlayColor, loadingMessage);
        LoadingView.Show(true);
        LoadingView.transform.SetAsLastSibling(); // поверх Viewport
    }

    public RuntimeLeaderbordScoreView CreateItem()
    {
        var itemGO = new GameObject("RuntimeLeaderbordScore", typeof(RectTransform));
        itemGO.transform.SetParent(ContentRect, false);
        var score = itemGO.AddComponent<RuntimeLeaderbordScoreView>();
        score.Build(_resolvedTMPFont, itemHeight, itemLeftPadding, itemRightPadding, fontSize);
        return score;
    }

    public void ClearItems()
    {
        for (int i = ContentRect.childCount - 1; i >= 0; i--)
            Destroy(ContentRect.GetChild(i).gameObject);
    }

    public void ShowLoading(bool visible)
    {
        if (LoadingView != null)
            LoadingView.Show(visible);
    }

    // Ждём готовности и скрываем лоадер
    public void WaitAndHideLoading(System.Func<bool> isReady)
    {
        if (_loadingRoutine != null) StopCoroutine(_loadingRoutine);
        _loadingRoutine = StartCoroutine(Co_WaitAndHide(isReady));
    }

    private System.Collections.IEnumerator Co_WaitAndHide(System.Func<bool> isReady)
    {
        if (LoadingView != null) LoadingView.Show(true);
        while (isReady == null || !isReady())
            yield return null;

        // if (LoadingView != null) LoadingView.Show(false);
        _loadingRoutine = null;
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(es);
        }
    }
}