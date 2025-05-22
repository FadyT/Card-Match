using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{

    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI GameOverText;
    private bool gameEnded = false;
    public GameObject gameOverPanel;
    public int totalCards;
    private int matchedCards = 0;
    public static GameManager Instance;

    public List<Card> flippedCards = new List<Card>();
    public int score = 0;

    public int currentWave = 1;
    public int maxWaves = 100;
    public float waveTime = 300f; // 5 minutes in seconds
    public int lives = 20;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI livesText;

    private float currentTime;
    private bool isTimerRunning = false;
    [SerializeField]
    private bool clearData = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }
    void Start()
    {
        currentTime = 300;
        isTimerRunning = false;
        if (clearData)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            StartWave();

        }
        else
        {
            var saved = SaveSystem.LoadGame();
            if (saved != null)
            {
                RestoreGame(saved);
            }
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.CeilToInt(currentTime) + "s";

            if (currentTime <= 0)
            {
                GameOver("Time's up!");
            }
        }
    }

    void StartWave()
    {
        matchedCards = 0;
        totalCards = 0;
        flippedCards.Clear();
        currentTime = waveTime;
        isTimerRunning = true;
        GameObject.FindObjectOfType<GridManager>().SetupWave(currentWave); // call new wave
        livesText.text = "Lives: " + lives;
        waveText.text = "Wave: " + currentWave;
    }

    public void CardMatched()
    {
        matchedCards += 2;

        if (matchedCards >= totalCards)
        {
            isTimerRunning = false;
            currentWave++;
            if (currentWave > maxWaves)
            {
                GameOver("You completed all waves!");
            }
            else
            {
                waveTime = Mathf.Max(30f, waveTime - 5f); // Decrease time by 5 sec for each wave
                lives = Mathf.Max(1, 20 - currentWave / 5); // Decrease lives each 5 waves
                StartCoroutine(NextWaveDelay());
            }
        }
    }

    IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(2f);
        StartWave();
    }

    void GameOver(string reason)
    {
        GameOverText.text = reason;
        isTimerRunning = false;
        gameEnded = true;
        Debug.Log("Game Over: " + reason);
        AudioManager.Instance.PlayGameOver();
        gameOverPanel.SetActive(true);
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

                    CardMatched();
                }
            }
            else
            {
                card1.Unflip();
                card2.Unflip();
                card1.MisMatch();
                card2.MisMatch();
                score -= 2;
                lives--;
                livesText.text = "Lives: " + lives;

                if (lives <= 0)
                {
                    GameOver("too many Mismatched cards");
                }

            }
            UpdateScoreText(score);

        }
    }

    public void SaveCurrentGame()
    {
        var data = new GameSaveData
        {
            totalCards = totalCards,
            matchedCards = matchedCards,
            score = score,
            wave = currentWave,
            remainingTime = currentTime,
            lives = lives,
            cards = new List<CardSaveData>()
        };

        foreach (Transform cardObj in GameObject.FindObjectOfType<GridManager>().gridParent)
        {
            Card card = cardObj.GetComponent<Card>();
            data.cards.Add(new CardSaveData
            {
                id = card.data.id,
                isFlipped = card.isFlipped,
                isMatched = card.isMatched
            });
        }

        SaveSystem.SaveGame(data);
    }
    void RestoreGame(GameSaveData data)
{
    score = data.score;
    currentWave = data.wave;
    lives = data.lives;
    currentTime = data.remainingTime;
    totalCards = data.totalCards;
    matchedCards = data.matchedCards;

    livesText.text = "Lives: " + lives;
    waveText.text = "Wave: " + currentWave;
    timerText.text = "Time: " + Mathf.CeilToInt(currentTime) + "s";
    scoreText.text = "Score: " + score.ToString();
        isTimerRunning = true;
    StartCoroutine(DelayedRestore(data));
}

IEnumerator DelayedRestore(GameSaveData data)
{
    GameObject.FindObjectOfType<GridManager>().SetupWave(currentWave);

    // Wait until cards are generated (1 frame delay is usually enough)
    yield return new WaitForSeconds(0.1f);

    GameObject.FindObjectOfType<GridManager>().GenerateSavedCards(data.cards);
}


    void OnApplicationPause(bool pause)
{
    if (pause) SaveCurrentGame();
}

void OnApplicationQuit()
{
    SaveCurrentGame();
}


}
