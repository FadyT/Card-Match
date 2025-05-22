using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    // combo
    public int comboCount = 0;

    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI UI_MotivationText;
    public TextMeshProUGUI UI_WaveText;
    public TextMeshProUGUI UI_ScoreText;
    public GameObject UI_GameOverPanel;
    public TextMeshProUGUI UI_TimerText;
    public TextMeshProUGUI UI_LivesText;

    [Header("Gameplay Settings")]
    public int maxWaves = 100;
    public float initialWaveTime = 300f;
    public int initialLives = 20;

    [Header("State")]
    public int totalCards;
    public int score;
    public int currentWave = 1;
    public int lives;

    private float currentTime;
    private bool isTimerRunning = false;
    private bool gameEnded = false;
    public int matchedCards = 0;

    private List<Card> flippedCards = new List<Card>();

    private const float WaveTimeDecrement = 5f;
    private const int LivesDecrementRate = 5;
    private const float DelayBeforeNextWave = 2f;
    private const float DelayBeforeMatchCheck = 0.5f;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentTime = initialWaveTime;

        if (HasClearData())
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            StartWave();
        }
        else
        {
            var saved = SaveSystem.LoadGame();
            if (saved != null) RestoreGame(saved);
            else StartWave();
        }
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UI_TimerText.text = "Time: " + Mathf.CeilToInt(currentTime) + "s";

        if (currentTime <= 0)
            TriggerGameOver("Time's up!");
    }

    private void OnApplicationPause(bool pause) => SaveIfNeeded(pause);
    private void OnApplicationQuit() => SaveIfNeeded(true);

    private void SaveIfNeeded(bool shouldSave)
    {
        if (shouldSave && !gameEnded)
            SaveCurrentGame();
    }

    #endregion

    #region Wave Handling

    private void StartWave()
    {

        gameEnded = false;
        flippedCards.Clear();
        matchedCards = 0;
        totalCards = 0;
        currentTime = initialWaveTime - currentWave * 5;
        isTimerRunning = true;

        GridManager.Instance.SetupWave(currentWave);
        UpdateUI();
    }

    private IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(DelayBeforeNextWave);
        StartWave();
    }

    public void CardMatched()
    {
        matchedCards += 2;

        if (matchedCards < totalCards) return;

        isTimerRunning = false;
        currentWave++;

        if (currentWave > maxWaves)
        {
            TriggerGameOver("You completed all waves!");
        }
        else
        {
            initialWaveTime = Mathf.Max(30f, initialWaveTime - WaveTimeDecrement);
            lives = Mathf.Max(1, initialLives - currentWave / LivesDecrementRate);
            StartCoroutine(NextWaveDelay());
        }
    }

    #endregion

    #region Card Logic

    public void CardFlipped(Card card)
    {
        if (gameEnded || card.isFlipped || card.isMatched || flippedCards.Contains(card)) return;

        flippedCards.Add(card);
        card.Flip();

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch(flippedCards[0], flippedCards[1]));
            flippedCards.Clear();
        }
    }

    private readonly string[] motivationWords = { "Great!", "Good job!", "Awesome!", "Well done!", "Fantastic!" };


    private IEnumerator CheckMatch(Card card1, Card card2)
    {
        yield return new WaitForSeconds(DelayBeforeMatchCheck);

        if (card1.data.id == card2.data.id)
        {
            card1.SetMatched(true);
            card2.SetMatched(true);
            card1.Match();
            card2.Match();

            matchedCards += 2;

            score += comboCount * 5 +10;
            comboCount++;

            if (comboCount > 1)
            {
                ShowMotivationText(comboCount);
            }

            if (matchedCards >= totalCards)
            {
                SaveCurrentGame();
                gameEnded = true;
                yield return new WaitForSeconds(1f);
                CardMatched();
            }
        }
        else
        {
            comboCount = 0;
            card1.Unflip();
            card2.Unflip();
            card1.MisMatch();
            card2.MisMatch();

            score = Mathf.Max(0, score - 2);
            lives--;

            if (lives <= 0)
                TriggerGameOver("Too many mismatched cards");
        }

        UpdateUI();
    }


    private void ShowMotivationText(int comboCount)
    {
        // Pick a random motivation word from array
        string message = motivationWords[Random.Range(0, motivationWords.Length)];

        message += $" x{comboCount}";

        UI_MotivationText.text = message;
        UI_MotivationText.gameObject.SetActive(true);

        // Hide after 1.5 seconds
        StartCoroutine(HideMotivationText());
    }

private IEnumerator HideMotivationText()
{
    yield return new WaitForSeconds(0.25f);
    UI_MotivationText.gameObject.SetActive(false);
}

    #endregion

    #region Game Over

    private void TriggerGameOver(string reason)
    {
        SaveSystem.SetClearDataFlag(true);
        isTimerRunning = false;
        gameEnded = true;
        AudioManager.Instance?.PlayGameOver();
        UI_GameOverPanel.SetActive(true);
        Debug.Log("Game Over: " + reason);
    }

    #endregion

    #region Save / Load

    private void SaveCurrentGame()
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


        foreach (Card card in GridManager.Instance.GetActiveCards())
        {
            data.cards.Add(new CardSaveData
            {
                id = card.data.id,
                isFlipped = card.isFlipped,
                isMatched = card.isMatched
            });
        Debug.Log("GameManagerSaving" + card.isFlipped + " : " + card.isMatched);

        }



        SaveSystem.SaveGame(data);
    }

    private void RestoreGame(GameSaveData data)
    {
        
        score = data.score;
        currentWave = data.wave;
        lives = data.lives;
        currentTime = data.remainingTime;
        totalCards = data.totalCards;
        matchedCards = data.matchedCards;
        
        UpdateUI();
        isTimerRunning = true;
        StartCoroutine(DelayedRestore(data));

    }

    private IEnumerator DelayedRestore(GameSaveData data)
    {
        GridManager.Instance.SetupWave(currentWave);
        yield return new WaitForSeconds(0.1f);
        GridManager.Instance.GenerateSavedCards(data.cards);

        if (totalCards <= matchedCards)
        {
            CardMatched();
        }
    }

    private bool HasClearData()
    {
        var data = SaveSystem.LoadGame();
        return data != null && data.clearData;
    }

    #endregion

    #region UI

    private void UpdateUI()
    {
        UI_LivesText.text = "Lives: " + lives;
        UI_WaveText.text = "Wave: " + currentWave;
        UI_TimerText.text = "Time: " + Mathf.CeilToInt(currentTime) + "s";
        UI_ScoreText.text = "Score: " + score;
    }

    #endregion
}
