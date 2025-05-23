using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 2;
    public int columns = 3;

    [Header("Card Setup")]
    public List<CardData> cardDataList;
    public GameObject cardPrefab;
    public Transform gridParent;

    private GridLayoutGroup gridLayout;
    private Queue<GameObject> cardPool = new Queue<GameObject>();

    public static GridManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        gridLayout = gridParent.GetComponent<GridLayoutGroup>();

        int totalCards = rows * columns;
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Total cards must be even for matching pairs.");
            return;
        }

        GenerateCards(totalCards);
        StartCoroutine(DelayedSetupGridLayout());
    }   

    private IEnumerator DelayedSetupGridLayout()
    {
        yield return null;
        SetupGridLayout();
    }

    public void SetupWave(int waveNumber)
    {
        ClearOldCards();

        int maxCards = 100;
        int baseTotal = Mathf.Min(maxCards, Mathf.Max(4, 2 + waveNumber * 2));
        if (baseTotal % 2 != 0) baseTotal++;

        (rows, columns) = FindBestGrid(baseTotal);

        GenerateCards(baseTotal);
        StartCoroutine(DelayedSetupGridLayout());
    }

    public void GenerateSavedCards(List<CardSaveData> savedCards)
    {
        ClearOldCards();

        foreach (var cardData in savedCards)
        {
            GameObject cardObj;
            if (cardPool.Count > 0)
            {
                cardObj = cardPool.Dequeue();
                cardObj.SetActive(true);
            }
            else
            {
                cardObj = Instantiate(cardPrefab);
            }
            cardObj.transform.SetParent(gridParent, false);
            var card = cardObj.GetComponent<Card>();
            card.data = cardDataList.Find(d => d.id == cardData.id);

            card.isMatched = cardData.isMatched;
            Debug.Log(cardData.isFlipped + " : " + cardData.isMatched);
            card.isFlipped = cardData.isFlipped;

            if (card.isMatched)
            {
                card.animator.SetTrigger("FlipToFront");

            }
            else
            {
                card.isFlipped = false;
                card.animator.SetTrigger("FlipToBack");
            }


            var frontImage = card.front.GetComponent<Image>();

            if (frontImage != null && cardObj.GetComponent<Card>().data != null)
            {
                frontImage.sprite = cardObj.GetComponent<Card>().data.image;
                frontImage.type = Image.Type.Simple;
                frontImage.preserveAspect = true;
                print("preserve aspect : " + frontImage.preserveAspect);
            }                
            
        }

        GameManager.Instance.totalCards = savedCards.Count;
        StartCoroutine(DelayedSetupGridLayout());
    }

    private void GenerateCards(int totalCards)
    {
        if (cardDataList.Count == 0)
        {
            Debug.LogError("No card data provided.");
            return;
        }

        var allCards = CreateShuffledPairs(totalCards / 2);

        foreach (var data in allCards)
        {

           GameObject cardObj;
            if (cardPool.Count > 0)
            {
                cardObj = cardPool.Dequeue();
                cardObj.SetActive(true);
            }
            else
            {
                cardObj = Instantiate(cardPrefab ,gridParent );
            }
            cardObj.GetComponent<Card>().data = data;

            var frontImage = cardObj.GetComponent<Card>().front.GetComponent<Image>();
            if (frontImage != null && cardObj.GetComponent<Card>().data != null)
            {
                frontImage.sprite = cardObj.GetComponent<Card>().data.image;
                frontImage.type = Image.Type.Simple;
                frontImage.preserveAspect = true;
                print("preserve aspect : " + frontImage.preserveAspect);
            }    
        }

        GameManager.Instance.totalCards = allCards.Count;
    }

    private List<CardData> CreateShuffledPairs(int numPairs)
    {
        var selected = new List<CardData>();
        var result = new List<CardData>();

        if (cardDataList.Count >= numPairs)
        {
            selected.AddRange(cardDataList);
            Shuffle(selected);

            for (int i = 0; i < numPairs; i++)
            {
                result.Add(selected[i]);
                result.Add(selected[i]);
            }
        }
        else
        {
            for (int i = 0; i < numPairs; i++)
            {
                var rand = cardDataList[Random.Range(0, cardDataList.Count)];
                result.Add(rand);
                result.Add(rand);
            }
        }

        Shuffle(result);
        return result;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private void ClearOldCards()
    {
        foreach (Transform child in gridParent)
        {
            child.gameObject.SetActive(false); // Pool instead of destroy
            cardPool.Enqueue(child.gameObject);
        }
    }

    private void SetupGridLayout()
    {
        if (gridLayout == null) return;

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;

        RectTransform parentRect = gridParent.parent.GetComponent<RectTransform>();
        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        float totalSpacingX = (columns - 1) * spacingX;
        float totalSpacingY = (rows - 1) * spacingY;

        float panelWidth = parentRect.rect.width;
        float panelHeight = parentRect.rect.height;

        float cellWidth = (panelWidth - totalSpacingX - gridLayout.padding.left - gridLayout.padding.right) / columns;
        float cellHeight = (panelHeight - totalSpacingY - gridLayout.padding.top - gridLayout.padding.bottom) / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }
    // add this as card saving break after using object booling 
    public List<Card> GetActiveCards()
    {
        List<Card> activeCards = new List<Card>();
        foreach (Transform child in gridParent)
        {
            if (child.gameObject.activeInHierarchy)
            {
                var card = child.GetComponent<Card>();
                if (card != null)
                {
                    activeCards.Add(card);
                }
            }
        }
        return activeCards;
    }

    private (int, int) FindBestGrid(int total)
    {
        int bestRows = 2, bestCols = total / 2;
        float bestDiff = Mathf.Abs(bestRows - bestCols);

        for (int r = 2; r <= 10; r++)
        {
            for (int c = 2; c <= 10; c++)
            {
                if (r * c == total)
                {
                    float diff = Mathf.Abs(r - c);
                    if (diff < bestDiff)
                    {
                        bestRows = r;
                        bestCols = c;
                        bestDiff = diff;
                    }
                }
            }
        }

        return (bestRows, bestCols);
    }
}
