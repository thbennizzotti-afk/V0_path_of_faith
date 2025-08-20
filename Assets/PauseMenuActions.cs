// Assets/Scripts/UI/Save/PauseMenuActions.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuActions : MonoBehaviour
{
    public GameObject saveSlotsPanel; // Panel avec PauseSaveSlotsUI

    public void OnClickSave()
    {
        if (saveSlotsPanel) saveSlotsPanel.SetActive(true);
    }

    public void OnClickQuitToMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
