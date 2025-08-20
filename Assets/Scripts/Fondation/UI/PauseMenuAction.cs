using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PauseMenuActions : MonoBehaviour
{
    [Header("Panneaux")]
    [SerializeField] GameObject mainPanel;          // PauseManPanel
    [SerializeField] GameObject saveSlotsPanel;     // SaveSlotsPanel

    [Header("Composants")]
    [SerializeField] PauseSaveSlotsUI saveSlotsUI;  // composant sur SaveSlotsPanel
    [SerializeField] PauseManager pauseManager;     // auto-récupéré si null

    [Header("Scène de retour")]
    [SerializeField] string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        if (pauseManager == null)
            pauseManager = GetComponent<PauseManager>();

        // Sécurité minimale : si on a un SaveSlotsPanel mais pas de ref, tente de la récupérer
        if (saveSlotsUI == null && saveSlotsPanel != null)
            saveSlotsUI = saveSlotsPanel.GetComponent<PauseSaveSlotsUI>();
    }

    // Bouton "Sauvegarder"
    public void OnClickSave()
    {
        if (pauseManager == null) return;

        if (saveSlotsUI != null)
            saveSlotsUI.Refresh();   // met à jour les infos

        pauseManager.ShowSaveSlots();
    }

    // Bouton "Retour" (sur le panneau des slots)
    public void OnClickBack()
    {
        if (pauseManager == null) return;
        pauseManager.ShowMainPanel();
    }

    // Bouton "Quitter"
    public void OnClickQuitToMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[PauseMenuActions] Nom de scène principal vide !");
            return;
        }

        // Désactiver la pause avant de changer de scène
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // (Optionnel) Méthodes utilitaires si tu veux les appeler ailleurs
    public void ShowMainPanel()    => pauseManager?.ShowMainPanel();
    public void ShowSaveSlots()    => pauseManager?.ShowSaveSlots();
}
