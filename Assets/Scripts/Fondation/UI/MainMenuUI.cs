using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MainMenuUI : MonoBehaviour
{
    [Header("Nom EXACT de la scène de jeu (Build Settings)")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Références UI (à brancher dans l'Inspector)")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject loadSlotsPanel; // panel des slots (désactivé par défaut)

    private void Awake()
    {
        // Câblage explicite, zéro surprise
        if (newGameButton)  { newGameButton.onClick.RemoveAllListeners();  newGameButton.onClick.AddListener(OnClickNewGame); }
        if (loadGameButton) { loadGameButton.onClick.RemoveAllListeners(); loadGameButton.onClick.AddListener(OnClickLoad); }
        if (quitButton)     { quitButton.onClick.RemoveAllListeners();     quitButton.onClick.AddListener(OnClickQuit); }
    }

    public void OnClickNewGame()
    {
        if (!IsSceneInBuild(gameSceneName))
        {
            Debug.LogError($"[MainMenuUI] La scène '{gameSceneName}' n'est pas dans File > Build Settings.");
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickLoad()
    {
        if (!loadSlotsPanel)
        {
            Debug.LogWarning("[MainMenuUI] Aucun 'loadSlotsPanel' assigné → rien à afficher (crée le panel de slots et assigne-le).");
            return;
        }
        loadSlotsPanel.SetActive(true); // affichera ton menu de slots quand tu l’auras fait
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}
