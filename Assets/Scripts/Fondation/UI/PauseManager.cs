using UnityEngine;

[DisallowMultipleComponent]
public class PauseManager : MonoBehaviour
{
    [Header("Références panneaux")]
    [SerializeField] GameObject rootPanel;      // ex: UI_Pause  (parent contenant tout le menu pause)
    [SerializeField] GameObject mainPanel;      // ex: PauseMainPanel (boutons Sauvegarder / Quitter)
    [SerializeField] GameObject saveSlotsPanel; // ex: SaveSlotsPanel  (liste des slots)

    [Header("Touche")]
    public KeyCode toggleKey = KeyCode.Escape;

    public bool IsPaused => rootPanel != null && rootPanel.activeSelf;

    void Start()
    {
        // État initial : menu fermé
        if (rootPanel) rootPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(false);
        if (saveSlotsPanel) saveSlotsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("[Pause] Escape pressed");
            Toggle();
        }
    }

    // ---------- Cycle principal ----------
    public void Toggle()
    {
        if (!rootPanel) return;

        if (!rootPanel.activeSelf)      // Ouvrir menu → panneau principal
            OpenMain();
        else
            Close();                    // Fermer menu
    }

    public void OpenMain()
    {
        Debug.Log("[Pause] OpenMain");
        if (!rootPanel) return;

        Time.timeScale = 0f;
        rootPanel.SetActive(true);
        if (mainPanel)      mainPanel.SetActive(true);
        if (saveSlotsPanel) saveSlotsPanel.SetActive(false);

        // Déverrouiller le curseur pour cliquer l’UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void OpenSlots()
    {
        Debug.Log("[Pause] OpenSlots");
        if (!rootPanel) return;

        Time.timeScale = 0f;
        rootPanel.SetActive(true);
        if (mainPanel)      mainPanel.SetActive(false);
        if (saveSlotsPanel) saveSlotsPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void Close()
    {
        Debug.Log("[Pause] Close");
        if (saveSlotsPanel) saveSlotsPanel.SetActive(false);
        if (mainPanel)      mainPanel.SetActive(false);
        if (rootPanel)      rootPanel.SetActive(false);

        Time.timeScale = 1f;

        // (optionnel) Re-verrouiller le curseur selon ton gameplay
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible   = false;
    }

    // ---------- Méthodes de compatibilité (API attendue par tes autres scripts) ----------
    public void Resume()          => Close();
    public void ShowMainPanel()   => OpenMain();
    public void ShowSaveSlots()   => OpenSlots();

    // ---------- Affectations depuis l’Inspector ----------
    // (helpers au cas où tu veux binder depuis d’autres scripts)
    public void SetRoot(GameObject g)       => rootPanel = g;
    public void SetMain(GameObject g)       => mainPanel = g;
    public void SetSlots(GameObject g)      => saveSlotsPanel = g;
}
