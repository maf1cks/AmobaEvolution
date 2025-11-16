using Data;
using UI;
using UnityEngine;

public class LeaderbordManager : BaseManager
{
    [Header("Canvas for Leaderbord element")]
    [SerializeField] private Canvas _canvas;

    [Header("Data")]
    [SerializeField] private LeaderbordDataSO dataSO;
    [SerializeField] private bool autoPopulate = false;

    [Header("UI Overrides (опционально)")]
    [SerializeField] private Sprite backgroundSpriteOverride;
    [SerializeField] private Sprite defaultKillsIconOverride;

    [Header("Layout")]
    [SerializeField] private Vector2 boardSize = new Vector2(450, 500);
    [SerializeField] private int headerTopOffset = 100;
    [SerializeField] private int maxEntries = 20;

    [Header("Placement")]
    [Tooltip("Если указан, лидерборд станет ребёнком этого узла и выровняется по центру с заданным смещением.")]
    [SerializeField] private RectTransform placementTarget;
    [Tooltip("Смещение от центра placementTarget или от выбранного пресета якорей.")]
    [SerializeField] private Vector2 anchoredOffset = Vector2.zero;

    public enum AnchorPreset
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    [Tooltip("Используется, если placementTarget не задан.")]
    [SerializeField] private AnchorPreset anchorPreset = AnchorPreset.TopRight;

    public RuntimeLeaderbord RuntimeUI { get; private set; }

    private Sprite _defaultKillsIcon;
    private Sprite _resolvedBg;
    private bool _isPopulated;

    public override void Setup()
    {
        base.Setup();

        // Получаем Canvas из вашего менеджера (или создаём)
        _canvas = ParentEntryPoint.GetManager<IntroSceneCanvasManager>()?.TryGetCanvasByKey(CanvasViewKey.Menu)?.Canvas;
        if (_canvas == null)
        {
            Debug.LogError("Canvas for leaderbord not found! Now using new created Canvas - RuntimeCanvas");
            _canvas = CreateCanvas();
        }

        // Ассеты
        var assets = GetManager<LoadAssetsManager>();
        _resolvedBg = backgroundSpriteOverride != null ? backgroundSpriteOverride : assets?.GetLeaderbordBackground();
        _defaultKillsIcon = defaultKillsIconOverride != null ? defaultKillsIconOverride : assets?.GetLeaderbordKillsIcon();

        // Создаём UI
        RuntimeUI = RuntimeLeaderbord.Create(_canvas, _resolvedBg, boardSize, headerTopOffset);
        ApplyPlacement(RuntimeUI.GetComponent<RectTransform>());

        // Лоадер поверх
        RuntimeUI.ShowLoading(true);

        // Автоподстановка данных
        if (autoPopulate && dataSO != null)
            PopulateFromSO();

        // Ждём готовность и скрываем лоадер (если используете этот механизм)
        RuntimeUI.WaitAndHideLoading(() =>
        {
            bool assetsReady = _resolvedBg != null && _defaultKillsIcon != null;
            return assetsReady && _isPopulated;
        });
    }

    public void SetData(LeaderbordDataSO so, bool populateImmediately = true)
    {
        dataSO = so;
        if (populateImmediately)
            PopulateFromSO();
    }

    public void PopulateFromSO()
    {
        if (RuntimeUI == null) return;

        _isPopulated = false;
        RuntimeUI.ClearItems();

        if (dataSO == null || dataSO.Entries == null || dataSO.Entries.Count == 0)
            return;

        int count = Mathf.Min(maxEntries, dataSO.Entries.Count);
        for (int i = 0; i < count; i++)
        {
            var entry = dataSO.Entries[i];
            var item = RuntimeUI.CreateItem();
            var icon = entry.Icon != null ? entry.Icon : _defaultKillsIcon;
            item.SetData(entry.Name, entry.Kills, icon);
        }

        _isPopulated = true;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (RuntimeUI != null)
        {
            Destroy(RuntimeUI.gameObject);
            RuntimeUI = null;
        }
    }

    // Позволяет динамически поменять точку/пресет и переустановить позицию
    public void Reposition(RectTransform newPlacementTarget = null, Vector2? newOffset = null, AnchorPreset? newPreset = null)
    {
        if (newPlacementTarget != null) placementTarget = newPlacementTarget;
        if (newOffset.HasValue) anchoredOffset = newOffset.Value;
        if (newPreset.HasValue) anchorPreset = newPreset.Value;

        if (RuntimeUI != null)
            ApplyPlacement(RuntimeUI.GetComponent<RectTransform>());
    }

    // Установка позиции: либо привязка к placementTarget, либо к Canvas по пресету якорей
    private void ApplyPlacement(RectTransform lbRT)
    {
        if (lbRT == null) return;

        if (placementTarget != null)
        {
            // Становимся ребёнком точки, выравниваем по центру
            lbRT.SetParent(placementTarget, false);
            lbRT.anchorMin = new Vector2(0.5f, 0.5f);
            lbRT.anchorMax = new Vector2(0.5f, 0.5f);
            lbRT.pivot = new Vector2(0.5f, 0.5f);
            lbRT.anchoredPosition = anchoredOffset;
            return;
        }

        // Иначе — остаёмся под Canvas и ставим по пресету якорей
        lbRT.SetParent(_canvas.transform, false);
        SetAnchorsAndPivotByPreset(lbRT, anchorPreset);
        lbRT.anchoredPosition = anchoredOffset;
    }

    private void SetAnchorsAndPivotByPreset(RectTransform rt, AnchorPreset preset)
    {
        switch (preset)
        {
            case AnchorPreset.TopLeft:
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                break;
            case AnchorPreset.TopCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                break;
            case AnchorPreset.TopRight:
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                break;
            case AnchorPreset.MiddleLeft:
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                break;
            case AnchorPreset.MiddleCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
            case AnchorPreset.MiddleRight:
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                break;
            case AnchorPreset.BottomLeft:
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                break;
            case AnchorPreset.BottomCenter:
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                break;
            case AnchorPreset.BottomRight:
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                break;
        }
    }

    private Canvas CreateCanvas()
    {
        var go = new GameObject("RuntimeCanvas", typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<UnityEngine.UI.CanvasScaler>();
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return canvas;
    }
}