using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class CanvasUIManagerBase : BaseManager
{
    [Header("Views registry")]
    [SerializeField] protected List<CanvasViewBase> views = new List<CanvasViewBase>();

    [Serializable]
    public struct ViewEntry
    {
        public CanvasViewKey Key;
        public CanvasViewBase View;
    }

    [SerializeField] protected CanvasViewBase initialView;

    protected readonly Dictionary<CanvasViewKey, CanvasViewBase> _map = new Dictionary<CanvasViewKey, CanvasViewBase>();
    public CanvasViewBase CurrentView { get; private set; }

    [Header("Transitions (optional)")]
    [SerializeField] protected bool transitionsEnabled = false;

    public enum TransitionMode { FadeCanvasGroup = 0, IrisCircle = 1 }
    [SerializeField] protected TransitionMode transitionMode = TransitionMode.IrisCircle;

    [SerializeField] protected Canvas transitionCanvas; // если null — берём Canvas текущего View или первый попавшийся

    [Header("Fade settings")]
    [SerializeField] protected Color fadeColor = Color.black;
    [SerializeField] protected float fadeInDuration = 0.5f;
    [SerializeField] protected float fadeOutDuration = 0.5f;
    [SerializeField] protected float blackScreenDelay = 0.15f;

    [Header("Iris settings")]
    [Tooltip("Круглый спрайт. Если пусто — будет создан процедурно.")]
    [SerializeField] protected Sprite irisSprite;
    [Tooltip("Целевой масштаб круга. Если <= 0 — подберется автоматически под экран.")]
    [SerializeField] protected float irisTargetScale = 0f;
    [SerializeField] protected float irisInDuration = 0.5f;
    [SerializeField] protected float irisOutDuration = 0.5f;
    [SerializeField] protected Color irisColor = Color.black;
    [SerializeField] protected int irisBaseSize = 256; // базовый размер круга (px) до масштабирования
    [SerializeField] protected float irisOvershoot = 1.1f; // запас, чтобы гарантированно закрыть углы

    // Runtime overlay
    private GameObject _overlayGO;
    private RectTransform _overlayRT;
    private CanvasGroup _overlayCG;
    private Image _overlayImage;       // используется для Fade
    private Coroutine _transitionRoutine;

    // Iris runtime (круг)
    private GameObject _irisGO;
    private RectTransform _irisRT;
    private Image _irisImage;
    private Canvas _overlayCanvasRef;

    public override void Setup()
    {
        base.Setup();

        foreach (var v in views)
            SafeAddView(v);

        HideAllViews();

        if (initialView != null)
            ShowView(initialView);
        else if (_map.TryGetValue(CanvasViewKey.Menu, out var menu))
            ShowView(menu);
    }

    protected void SafeAddView(CanvasViewBase v)
    {
        if (v == null) return;
        if (!views.Contains(v))
            views.Add(v);
        v.CanvasParentManager = this;

        if (v.Key != CanvasViewKey.None)
            RegisterView(v.Key, v, overwrite: false);
    }

    public void RegisterView(CanvasViewKey key, CanvasViewBase view, bool overwrite)
    {
        if (view == null || key == CanvasViewKey.None) return;

        view.CanvasParentManager = this;
        if (_map.TryGetValue(key, out var existing))
        {
            if (existing == view) return;
            if (!overwrite)
            {
                Debug.LogWarning($"[CanvasUIManagerBase] View key {key} already registered with {existing.name}. Skipping {view.name}.");
                return;
            }
        }
        _map[key] = view;
        if (!views.Contains(view))
            views.Add(view);
    }

    public virtual bool ShowView(CanvasViewKey key)
    {
        if (!_map.TryGetValue(key, out var view) || view == null)
        {
            Debug.LogWarning($"[CanvasUIManagerBase] View for key {key} not found.");
            return false;
        }
        ShowView(view);
        return true;
    }

    public virtual void ShowView(CanvasViewBase view)
    {
        if (view == null) return;
        if (view == CurrentView) return;

        if (CurrentView != null)
            CurrentView.Hide();

        if (!views.Contains(view))
            views.Add(view);

        view.CanvasParentManager = this;
        view.Show();

        CurrentView = view;
    }

    public virtual void HideCurrent()
    {
        if (CurrentView == null) return;
        CurrentView.Hide();
        CurrentView = null;
    }

    public virtual void HideAllViews()
    {
        foreach (var v in views)
            if (v != null) v.Hide();
        CurrentView = null;
    }

    public virtual CanvasViewBase TryGetCanvasByKey(CanvasViewKey key)
    {
        foreach (var kv in _map)
            if (kv.Key == key) return kv.Value;
        return null;
    }

    public virtual T TryGetCanvasByType<T>() where T : CanvasViewBase
    {
        foreach (var kv in _map)
            if (kv.Value is T t) return t;
        return null;
    }

    // ================= Unified Transitions API (SetView) =================

    // Показ/скрытие по ключу
    public void SetView(CanvasViewKey key, CanvasStateType state = CanvasStateType.Show, Action onCompleted = null)
    {
        CanvasViewBase view = null;
        if (state == CanvasStateType.Show)
            view = TryGetCanvasByKey(key); // целевой View для показа
        SetView(view, state, onCompleted);
    }

    // Показ/скрытие по ссылке
    public void SetView(CanvasViewBase view, CanvasStateType state = CanvasStateType.Show, Action onCompleted = null)
    {
        // Без анимации или явно None — мгновенно
        if (state == CanvasStateType.None || !transitionsEnabled)
        {
            if (state == CanvasStateType.Hide)
            {
                HideCurrent();
                onCompleted?.Invoke();
                return;
            }

            // Show мгновенно: выключить текущий и включить новый
            if (view != null) ShowView(view);
            onCompleted?.Invoke();
            return;
        }

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);
        _transitionRoutine = StartCoroutine(Co_SetView(view, state, onCompleted));
    }

    // Упрощённый вызов: только состояние (например, скрыть текущий)
    public void SetView(CanvasStateType state = CanvasStateType.Show, Action onCompleted = null)
    {
        if (state == CanvasStateType.Hide)
        {
            SetView((CanvasViewBase)null, CanvasStateType.Hide, onCompleted);
            return;
        }

        var viewToShow = initialView != null ? initialView : CurrentView;
        if (viewToShow == null)
        {
            onCompleted?.Invoke();
            return;
        }
        SetView(viewToShow, CanvasStateType.Show, onCompleted);
    }

    // Совместимость: старые методы-обёртки
    public void ShowViewWithTransition(CanvasViewKey key) => SetView(key, CanvasStateType.Show, null);
    public void ShowViewWithTransition(CanvasViewBase view) => SetView(view, CanvasStateType.Show, null);
    public void DisableCurrentViewWithTransition(Action onCompleted = null) => SetView(CanvasStateType.Hide, onCompleted);

    // ================= Internal coroutines =================

    private IEnumerator Co_SetView(CanvasViewBase targetView, CanvasStateType state, Action onCompleted)
    {
        EnsureOverlay();

        // 1) Закрываем (к «чёрному экрану»)
        switch (transitionMode)
        {
            case TransitionMode.FadeCanvasGroup:
                yield return StartCoroutine(Co_Fade(0f, 1f, fadeInDuration));
                break;

            case TransitionMode.IrisCircle:
                // Увеличиваем круг до полного перекрытия экрана
                yield return StartCoroutine(Co_Iris(0.01f, irisTargetScale, irisInDuration));
                break;
        }

        // 2) На «чёрном экране»:
        // - при Hide: выключаем текущий
        // - при Show: выключаем текущий и включаем целевой (ShowView сам скрывает предыдущий)
        if (state == CanvasStateType.Hide)
        {
            HideCurrent();
        }
        else if (state == CanvasStateType.Show && targetView != null)
        {
            ShowView(targetView); // внутри спрячется текущий и включится новый
        }

        if (blackScreenDelay > 0f)
            yield return new WaitForSeconds(blackScreenDelay);

        // 3) Открываем обратно
        switch (transitionMode)
        {
            case TransitionMode.FadeCanvasGroup:
                yield return StartCoroutine(Co_Fade(1f, 0f, fadeOutDuration));
                break;

            case TransitionMode.IrisCircle:
                // Уменьшаем круг обратно до маленького
                yield return StartCoroutine(Co_Iris(GetCurrentIrisScale(), 0.01f, irisOutDuration));
                break;
        }

        HideOverlay();
        _transitionRoutine = null;
        onCompleted?.Invoke();
    }

    // ================= Overlay/Iris helpers =================

    private void EnsureOverlay()
    {
        // Родительский Canvas
        var parentCanvas = transitionCanvas != null ? transitionCanvas :
                           CurrentView != null ? CurrentView.Canvas :
                           UnityEngine.Object.FindObjectOfType<Canvas>();

        if (parentCanvas == null)
        {
            Debug.LogWarning("[CanvasUIManagerBase] No Canvas found for transition overlay. Creating temporary Canvas.");
            var go = new GameObject("TransitionCanvas", typeof(RectTransform));
            parentCanvas = go.AddComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            transitionCanvas = parentCanvas;
        }
        _overlayCanvasRef = parentCanvas.rootCanvas;

        if (_overlayGO == null)
        {
            _overlayGO = new GameObject("TransitionOverlay", typeof(RectTransform));
            _overlayGO.transform.SetParent(parentCanvas.transform, false);
            _overlayRT = _overlayGO.GetComponent<RectTransform>();
            _overlayRT.anchorMin = Vector2.zero;
            _overlayRT.anchorMax = Vector2.one;
            _overlayRT.pivot = new Vector2(0.5f, 0.5f);
            _overlayRT.offsetMin = Vector2.zero;
            _overlayRT.offsetMax = Vector2.zero;

            _overlayCG = _overlayGO.AddComponent<CanvasGroup>();
            _overlayCG.blocksRaycasts = true; // блокируем инпут поверх

            _overlayImage = _overlayGO.AddComponent<Image>(); // для Fade
            _overlayGO.SetActive(false);
        }

        _overlayGO.transform.SetAsLastSibling();
        _overlayGO.SetActive(true);

        if (transitionMode == TransitionMode.FadeCanvasGroup)
        {
            // Полноэкранный прямоугольник с альфой
            _overlayImage.enabled = true;
            _overlayImage.sprite = null;
            _overlayImage.raycastTarget = true;
            _overlayImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            _overlayCG.alpha = 0f;

            // Iris-специфика: скрыть круг, если он был создан
            if (_irisGO != null) _irisGO.SetActive(false);
        }
        else // IrisCircle
        {
            // Основной прямоугольник — прозрачный (только для блокировки инпута)
            _overlayImage.enabled = false;
            _overlayCG.alpha = 1f;

            // Создаем/настраиваем круг
            if (_irisGO == null)
            {
                _irisGO = new GameObject("IrisCircle", typeof(RectTransform));
                _irisGO.transform.SetParent(_overlayGO.transform, false);
                _irisRT = _irisGO.GetComponent<RectTransform>();
                _irisRT.anchorMin = new Vector2(0.5f, 0.5f);
                _irisRT.anchorMax = new Vector2(0.5f, 0.5f);
                _irisRT.pivot = new Vector2(0.5f, 0.5f);
                _irisRT.sizeDelta = new Vector2(irisBaseSize, irisBaseSize);

                _irisImage = _irisGO.AddComponent<Image>();
                _irisImage.raycastTarget = true;
                _irisImage.preserveAspect = true;
            }

            _irisGO.SetActive(true);
            _irisImage.color = irisColor;
            _irisImage.type = Image.Type.Simple;
            _irisImage.sprite = irisSprite != null ? irisSprite : CreateProceduralCircleSprite(irisBaseSize);

            // Начальный маленький масштаб
            _irisRT.localScale = Vector3.one * 0.01f;
        }
    }

    private void HideOverlay()
    {
        if (_overlayGO != null)
            _overlayGO.SetActive(false);
    }

    private IEnumerator Co_Fade(float from, float to, float duration)
    {
        if (_overlayCG == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? t / duration : 1f;
            _overlayCG.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        _overlayCG.alpha = to;
    }

    private IEnumerator Co_Iris(float fromScale, float toScaleParam, float duration)
    {
        if (_irisRT == null) yield break;

        // Автоподбор масштаба, если не задан
        float target = toScaleParam;
        if (target <= 0f)
            target = ComputeIrisTargetScale() * irisOvershoot;

        float t = 0f;
        float start = fromScale;
        float end = target;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? t / duration : 1f;
            float s = Mathf.Lerp(start, end, k);
            _irisRT.localScale = Vector3.one * s;
            yield return null;
        }
        _irisRT.localScale = Vector3.one * end;
    }

    private float GetCurrentIrisScale()
    {
        return _irisRT != null ? _irisRT.localScale.x : 1f;
    }

    private float ComputeIrisTargetScale()
    {
        // Диагональ экрана в пикселях
        var canvas = _overlayCanvasRef != null ? _overlayCanvasRef : (transitionCanvas != null ? transitionCanvas.rootCanvas : null);
        float w = Screen.width;
        float h = Screen.height;

        if (canvas != null)
        {
            // Для безопасности берём пиксельный размер корневого Canvas
            w = canvas.pixelRect.width;
            h = canvas.pixelRect.height;
        }

        float diagonal = Mathf.Sqrt(w * w + h * h);
        float baseDiameter = irisBaseSize; // т.к. sizeDelta = (irisBaseSize, irisBaseSize)
        if (baseDiameter <= 0f) baseDiameter = 256f;

        // Масштаб, чтобы диаметр круга >= диагонали экрана
        float scale = diagonal / baseDiameter;
        return Mathf.Max(1f, scale);
    }

    private Sprite CreateProceduralCircleSprite(int size)
    {
        size = Mathf.Clamp(size, 8, 2048);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[size * size];
        float r = (size - 1) * 0.5f;
        float r2 = r * r;

        Color32 col = new Color32(
            (byte)(irisColor.r * 255f),
            (byte)(irisColor.g * 255f),
            (byte)(irisColor.b * 255f),
            255);

        for (int y = 0; y < size; y++)
        {
            float dy = y - r;
            for (int x = 0; x < size; x++)
            {
                float dx = x - r;
                float dist2 = dx * dx + dy * dy;
                bool inside = dist2 <= r2;
                pixels[y * size + x] = inside ? col : new Color32(0, 0, 0, 0); // круг с прозрачным фоном
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return sprite;
    }
}