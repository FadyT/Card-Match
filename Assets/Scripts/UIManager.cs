using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public GameObject resumButton;

    void Awake()
    {
        if (SaveSystem.HasSavedData())
        {   if (resumButton != null)
            {
                resumButton.SetActive(true);
            }
        }
        else
        {
            if (resumButton != null)
            {
                resumButton.SetActive(false);
            }
        }

    }

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
