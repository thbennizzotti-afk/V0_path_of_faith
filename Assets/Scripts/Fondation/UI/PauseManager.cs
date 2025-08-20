using UnityEngine;

[DisallowMultipleComponent]
public class PauseManager : MonoBehaviour
{
    [Header("Références panneaux")]
    [SerializeField] private GameObject rootPanel;      // UI_Pause (canvas racine de pause)
    [SerializeField] private GameObject mainPanel;      // PauseManPanel (boutons Sauvegarder / Quitter)
    [SerializeField] private GameObject saveSlotsPanel; // SaveSlotsPanel (choix de slot)

    [Header("Touche")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    public bool IsPaused => rootPanel != null && rootPanel.activeSelf;

    private void Start()
    {
        // Démarrage propre
        if (rootPanel) rootPanel.SetActive(false);
        if (saveSlotsPanel) saveSlotsPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);

        // S’assure qu’on n’est pas figé au lancement
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            TogglePause();
    }

    /// <summary>Basculer l’état de pause.</summary>
    public void TogglePause()
    {
        if (rootPanel == null) return;

        if (!IsPaused)
        {
            Pause();
            return;
        }

        // Déjà en pause : si on est sur les slots, revenir au menu principal,
        // sinon reprendre le jeu.
        if (saveSlotsPanel != null && saveSlotsPanel.activeSelf)
            ShowMainPanel();
        else
            Resume();
    }

    /// <summary>Ouvre la pause et affiche le panneau principal.</summary>
    public void Pause()
    {
        if (rootPanel) rootPanel.SetActive(true);
        ShowMainPanel();
        Time.timeScale = 0f;
    }

    /// <summary>Ferme complètement la pause et relance le temps.</summary>
    public void Resume()
    {
        if (rootPanel) rootPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>Affiche le panneau principal (Save / Quitter).</summary>
    public void ShowMainPanel()
    {
        if (mainPanel) mainPanel.SetActive(true);
        if (saveSlotsPanel) saveSlotsPanel.SetActive(false);
    }

    /// <summary>Affiche le panneau des slots de sauvegarde.</summary>
    public void ShowSaveSlots()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (saveSlotsPanel) saveSlotsPanel.SetActive(true);
    }

    // Sécurité : si jamais ce GameObject est désactivé en étant en pause,
    // on remet le timeScale à 1 pour ne pas “bloquer” l’éditeur/le jeu.
    private void OnDisable()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
