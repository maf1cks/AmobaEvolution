using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Data;
using UI;

public class MenuCanvas : CanvasViewBase
{
    [Header("Game objects")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private SpriteRenderer gameTile;
    [SerializeField] private IntroUIManager introUIManager;
    public CanvasGroup CanvasGroup;
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button magazinButton;
    [SerializeField] private Button arenaButton;
    [SerializeField] private Button zadanieButton;
    [SerializeField] private Button evolutionButton;
    [SerializeField] private Button gameButton;
    [SerializeField] private Button casinoButton;

    private void Start()
    {
        if (magazinButton)  magazinButton.onClick.AddListener(() => CanvasParentManager.ShowViewWithTransition(CanvasViewKey.Magazin));
        if (arenaButton)    arenaButton.onClick.AddListener(() => CanvasParentManager.ShowViewWithTransition(CanvasViewKey.Arena));
        if (zadanieButton)  zadanieButton.onClick.AddListener(() => CanvasParentManager.ShowViewWithTransition(CanvasViewKey.Zadanie));
        if (evolutionButton) evolutionButton.onClick.AddListener(() => CanvasParentManager.ShowViewWithTransition(CanvasViewKey.Evolution));
        if (casinoButton)   casinoButton.onClick.AddListener(() => CanvasParentManager.ShowViewWithTransition(CanvasViewKey.Casino));

        if (gameButton)
        {
            gameButton.onClick.AddListener(() =>
            {
                // Для геймплея есть доп. действия — используем общий переход
                CanvasParentManager.SetView(CanvasViewKey.Game, CanvasStateType.Show, () =>
                {
                    CanvasParentManager.HideAllViews();
                    if (playerPrefab) playerPrefab.SetActive(true);

                    if (introUIManager && gameTile)
                        introUIManager.SetUpSpritesByLevel(gameTile, introUIManager.level, 2);
                });
            });
        }
    }

    public void StartLoadingLeaderbord()
    {
        StartCoroutine(HideLoadingAfterDelay());
    }

    private IEnumerator HideLoadingAfterDelay()
    {
        // Опционально: показать лоадер сразу
        // EntryPoint.Instance.GetManager<LeaderbordManager>().RuntimeUI.ShowLoading(true);

        yield return new WaitForSeconds(2f); // или WaitForSecondsRealtime(2f), если нужно игнорировать Time.timeScale
        EntryPoint.Instance.GetManager<LeaderbordManager>().RuntimeUI.ShowLoading(false);
        EntryPoint.Instance.GetManager<LeaderbordManager>().PopulateFromSO();
    }
}