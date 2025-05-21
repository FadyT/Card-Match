using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{

    public TextMeshProUGUI scoreText;
    private bool gameEnded = false;
    public GameObject gameOverPanel;
    public int totalCards;
    private int matchedCards = 0;
    public static GameManager Instance;

    public List<Card> flippedCards = new List<Card>();
    public int score = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void UpdateScoreText(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }
    public void CardFlipped(Card card)
    {
        if (gameEnded) return;

        if (card.isFlipped || card.isMatched || flippedCards.Contains(card))
            return;

        flippedCards.Add(card);
        card.Flip();

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch(flippedCards[0], flippedCards[1]));
            flippedCards.Clear();
        }

        IEnumerator CheckMatch(Card card1, Card card2)
        {

            yield return new WaitForSeconds(0.5f);

            if (card1.data.id == card2.data.id)
            {
                card1.isMatched = true;
                card2.isMatched = true;
                card1.Match();
                card2.Match();
                score += 10;
                matchedCards += 2;
                card1.Match();
                card2.Match();

                if (matchedCards >= totalCards)
                {
                    yield return new WaitForSeconds(1f);

                    ShowGameOver();
                }
            }
            else
            {
                card1.Unflip();
                card2.Unflip();
                card1.MisMatch();
                card2.MisMatch();
                score -= 2;
            }
            UpdateScoreText(score);

        }
    }
    public void ShowGameOver()
    {
     Debug.Log("Game Over!");
     gameEnded = true;
     gameOverPanel.SetActive(true);
    }
}
