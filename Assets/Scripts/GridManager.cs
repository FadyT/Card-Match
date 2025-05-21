using UnityEngine;
using UnityEngine.UI;
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

    // Generate pairs without needing unique cards
    for (int i = 0; i < totalCards / 2; i++)
    {
        // Choose a random cardData (even if it's already used)
        var randomCard = cardDataList[Random.Range(0, cardDataList.Count)];

        // Add it twice to form a pair
        allCards.Add(randomCard);
        allCards.Add(randomCard);
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

            float spacing = 10f;
            float panelWidth = ((RectTransform)gridParent).rect.width;
            float panelHeight = ((RectTransform)gridParent).rect.height;

            float cellWidth = (panelWidth - (columns - 1) * spacing) / columns;
            float cellHeight = (panelHeight - (rows - 1) * spacing) / rows;

            gridLayout.spacing = new Vector2(spacing, spacing);
            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}
