using UnityEngine;

public class IntroUIManager : MonoBehaviour
{
    
    [SerializeField] public SpriteRenderer NormisSprite;
    [SerializeField] public SpriteRenderer TileSprite;
    [SerializeField] public CanvasGroup playerCanvasGroup;
    
    [SerializeField] public Sprite Tile1Sprite;
    [SerializeField] public Sprite NormisAmobaSprite;
    [SerializeField] public Sprite Game1Sprite;
    
    [SerializeField] public Sprite Tile2Sprite;
    [SerializeField] public Sprite NormisChervSprite;
    [SerializeField] public Sprite Game2Sprite;
    
    [SerializeField] public Sprite Tile3Sprite;
    [SerializeField] public Sprite NormisSkorpSprite;
    [SerializeField] public Sprite Game3Sprite;
    
    [SerializeField] public Sprite Tile4Sprite;
    [SerializeField] public Sprite NormisReksSprite;
    [SerializeField] public Sprite Game4Sprite;

    [SerializeField] public int level;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetAlphaGameUI(0);
        SetInteractableGameUI(false);
        SetUpSpritesByLevel(NormisSprite,level,3);
        SetUpSpritesByLevel(TileSprite,level,1);
    }

    public void SetUpSpritesByLevel(SpriteRenderer spriteRenderer,int level,int each)
    {
        switch (each)
        {
            case 1:
            {
                switch (level)
                {
                    case 1:
                    {
                        spriteRenderer.sprite = Tile1Sprite;
                        break;
                    }
                    case 2:
                    {
                        spriteRenderer.sprite = Tile2Sprite;
                        break;
                    }
                    case 3:
                    {
                        spriteRenderer.sprite = Tile3Sprite;
                        break;
                    }
                    case 4:
                    {
                        spriteRenderer.sprite = Tile4Sprite;
                        break;
                    }
                }
                break;
            }
            case 2:
            {
                switch (level)
                {
                    case 1:
                    {
                        spriteRenderer.sprite = Game1Sprite;
                        break;
                    }
                    case 2:
                    {
                        spriteRenderer.sprite = Game2Sprite;
                        break;
                    }
                    case 3:
                    {
                        spriteRenderer.sprite = Game3Sprite;
                        break;
                    }
                    case 4:
                    {
                        spriteRenderer.sprite = Game4Sprite;
                        break;
                    }
                }
                break;
            }
            case 3:
            {
                switch (level)
                {
                    case 1:
                    {
                        spriteRenderer.sprite = NormisAmobaSprite;
                        break;
                    }
                    case 2:
                    {
                        spriteRenderer.sprite = NormisChervSprite;
                        break;
                    }
                    case 3:
                    {
                        spriteRenderer.sprite = NormisSkorpSprite;
                        break;
                    }
                    case 4:
                    {
                        spriteRenderer.sprite = NormisReksSprite;
                        break;
                    }
                }
                break;
            }
                
                
        }
        
    }

    public void SetAlphaGameUI(int alpha)
    {
        playerCanvasGroup.alpha = alpha;
    }
    public void SetInteractableGameUI(bool interactable)
    {
        playerCanvasGroup.interactable = interactable;
    }

    public void OnAnimationEnded()
    {   
        SetAlphaGameUI(1);
        SetInteractableGameUI(true);
    }
}
