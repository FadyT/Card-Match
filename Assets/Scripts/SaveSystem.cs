using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/save.json";

    public static void SaveGame(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
    }

    public static GameSaveData LoadGame()
    {
        if (!File.Exists(savePath)) return null;
        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    public static void ClearSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
    }
}
[System.Serializable]
public class GameSaveData
{
    public int totalCards;
    public int matchedCards;
    public int score;
    public int wave;
    public float remainingTime;
    public int lives;
    public List<CardSaveData> cards;
}

[System.Serializable]
public class CardSaveData
{
    public int id;          // The card's data ID
    public bool isFlipped;  // Whether it is currently flipped
    public bool isMatched;  // Whether it's matched
}

