using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ValutaView : MonoBehaviour
{
    [SerializeField] private ValutaType valutaType = ValutaType.Coins;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image iconImage;

    private ValutaManager _valutaManager;
    private LoadAssetsManager _assets;
    private Coroutine _waitRoutine;

    private void OnEnable()
    {
        TryBindManagersAndSubscribe();

        // Если EntryPoint ещё не инициализирован — подождём
        if (_valutaManager == null || _assets == null)
            _waitRoutine = StartCoroutine(WaitForManagersAndSubscribe());
    }

    private void OnDisable()
    {
        if (_valutaManager != null)
            _valutaManager.AddValutaEvent -= OnValutaChanged;

        if (_waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
            _waitRoutine = null;
        }
    }

    private IEnumerator WaitForManagersAndSubscribe()
    {
        while (EntryPoint.Instance == null || !EntryPoint.Instance.IsInitialized)
            yield return null;

        TryBindManagersAndSubscribe();
        _waitRoutine = null;
    }

    private void TryBindManagersAndSubscribe()
    {
        if (EntryPoint.Instance == null) return;

        _valutaManager = EntryPoint.Instance.GetManager<ValutaManager>();
        _assets = EntryPoint.Instance.GetManager<LoadAssetsManager>();

        if (_valutaManager != null)
        {
            _valutaManager.AddValutaEvent -= OnValutaChanged; // на всяк. случай
            _valutaManager.AddValutaEvent += OnValutaChanged;
        }

        SetupIcon();
        RefreshAmount();
    }

    private void OnValutaChanged(ValutaType type)
    {
        if (type == valutaType)
            RefreshAmount();
    }

    private void RefreshAmount()
    {
        if (_valutaManager == null || amountText == null) return;
        amountText.text = _valutaManager.GetAmount(valutaType).ToString();
    }

    private void SetupIcon()
    {
        if (_assets == null || iconImage == null) return;
        var sprite = _assets.GetValutaSprite(valutaType);
        if (sprite != null)
        {
            iconImage.sprite = sprite;
            Debug.Log("Sprite: " + sprite);
        }
    }
}