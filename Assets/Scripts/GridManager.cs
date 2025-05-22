using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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

    void Start()
    {
        gridLayout = gridParent.GetComponent<GridLayoutGroup>();

        int totalCardsNeeded = rows * columns;

        if ((totalCardsNeeded % 2) != 0)
        {
            Debug.LogError("Total cards must be even for matching pairs.");
            return;
        }

        GenerateCards(totalCardsNeeded);
        StartCoroutine(DelayedSetupGridLayout());


    }

    private IEnumerator DelayedSetupGridLayout()
    {
        yield return null; // wait one frame so layout can be calculated
        SetupGridLayout();
    }


    void GenerateCards(int totalCards)
    {
        if (cardDataList.Count == 0)
        {
            Debug.LogError("cardDataList is empty. Cannot generate cards.");
            return;
        }

        var allCards = new List<CardData>();
        int numPairs = totalCards / 2;
        if (cardDataList.Count >= numPairs)
        {
            List<CardData> shuffled = new List<CardData>(cardDataList);
            Shuffle(shuffled);
            for (int i = 0; i < numPairs; i++)
            {
             allCards.Add(shuffled[i]);
             allCards.Add(shuffled[i]);
            }
        }
    else
    {
        // Not enough unique cards, allow duplicates
        for (int i = 0; i < numPairs; i++)
        {
            // Choose a random cardData (even if it's already used)
            var randomCard = cardDataList[Random.Range(0, cardDataList.Count)];

            // Add it twice to form a pair
            allCards.Add(randomCard);
            allCards.Add(randomCard);
        }
    }


        Shuffle(allCards);

        foreach (CardData data in allCards)
        {
            GameObject card = Instantiate(cardPrefab, gridParent);
            card.GetComponent<Card>().data = data;
        }

        GameManager.Instance.totalCards = allCards.Count;
    }



    void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            var temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
    void SetupGridLayout()
    {
        if (gridLayout != null)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;

            float spacing = gridLayout.spacing.x; // consistent spacing assumed
            float totalSpacingWidth = (columns - 1) * spacing;
            float totalSpacingHeight = (rows - 1) * spacing;

            RectTransform parentRect = gridParent.transform.parent.GetComponent<RectTransform>();

            float panelWidth = parentRect.rect.width;
            float panelHeight = parentRect.rect.height;

            float cellWidth = (panelWidth - totalSpacingWidth - gridLayout.padding.left - gridLayout.padding.right) / columns;
            float cellHeight = (panelHeight - totalSpacingHeight - gridLayout.padding.top - gridLayout.padding.bottom) / rows;

            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);

            Debug.Log($"Grid Size: {panelWidth}x{panelHeight}, Cell: {cellWidth}x{cellHeight}");
        }
    }


public void SetupWave(int waveNumber)
{
    ClearOldCards();

    int maxCards = 100;

    int baseTotal = 2 + waveNumber * 2;  
    if (baseTotal > maxCards) baseTotal = maxCards;

    // make sure total cards number is even
    if (baseTotal % 2 != 0) baseTotal++;

    // set totalCards that can fit the screen
    int bestRows = 2;
    int bestCols = baseTotal / 2;
    float bestRatioDiff = Mathf.Abs(bestRows - bestCols);

    for (int r = 2; r <= 10; r++)
    {
        for (int c = 2; c <= 10; c++)
        {
            if (r * c == baseTotal)
            {
                float ratioDiff = Mathf.Abs(r - c);
                if (ratioDiff < bestRatioDiff)
                {
                    bestRows = r;
                    bestCols = c;
                    bestRatioDiff = ratioDiff;
                }
            }
        }
    }

    rows = bestRows;
    columns = bestCols;

    GenerateCards(baseTotal);
    StartCoroutine(DelayedSetupGridLayout());
}


    void ClearOldCards()
{
    foreach (Transform child in gridParent)
    {
        Destroy(child.gameObject);
    }
}
}
