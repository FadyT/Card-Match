using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public void NewGame()
    {
        SaveSystem.SetClearDataFlag(true);

        SceneManager.LoadScene("Game");
    }
    public void Resume()
    {
        SaveSystem.SetClearDataFlag(false);
        SceneManager.LoadScene("Game");
    }
}
