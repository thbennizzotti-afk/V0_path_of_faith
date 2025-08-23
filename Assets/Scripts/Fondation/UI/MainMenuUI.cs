using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PathOfFaith.Fondation.Core; // PendingLoad

namespace PathOfFaith.Fondation.UI
{
    [DisallowMultipleComponent]
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Nom EXACT de la scène de jeu (Build Settings)")]
        [SerializeField] private string gameSceneName = "Game";

        [Header("Boutons principaux (optionnels si AutoFind)")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button quitButton;

        [Header("Panneau de sélection de slots (facultatif)")]
        [SerializeField] private GameObject loadSlotsPanel;
        [SerializeField] private Button loadSlot1Button;
        [SerializeField] private Button loadSlot2Button;
        [SerializeField] private Button loadSlot3Button;
        [SerializeField] private Button backFromSlotsButton;

        void Awake()
        {
            // Sécurité : au menu, on tourne à vitesse normale
            Time.timeScale = 1f;

            AutoFindIfNull();

            if (newGameButton)   newGameButton.onClick.AddListener(OnClickNewGame);
            if (loadGameButton)  loadGameButton.onClick.AddListener(OnClickLoadGame);
            if (quitButton)      quitButton.onClick.AddListener(OnClickQuit);

            if (loadSlot1Button) loadSlot1Button.onClick.AddListener(() => OnClickLoadSlot("slot1"));
            if (loadSlot2Button) loadSlot2Button.onClick.AddListener(() => OnClickLoadSlot("slot2"));
            if (loadSlot3Button) loadSlot3Button.onClick.AddListener(() => OnClickLoadSlot("slot3"));
            if (backFromSlotsButton) backFromSlotsButton.onClick.AddListener(HideSlotsPanel);

            if (loadSlotsPanel) loadSlotsPanel.SetActive(false);

            Debug.Log("[MainMenuUI] Initialisé.");
        }

        void AutoFindIfNull()
        {
            // Recherche "souple" par noms fréquents dans la hiérarchie
            Button FindBtn(string exactName)
            {
                foreach (var b in GetComponentsInChildren<Button>(true))
                    if (b && b.gameObject.name == exactName) return b;
                return null;
            }

            if (!newGameButton)   newGameButton   = FindBtn("New")        ?? FindBtn("Btn_New");
            if (!loadGameButton)  loadGameButton  = FindBtn("Load")       ?? FindBtn("Btn_Load");
            if (!quitButton)      quitButton      = FindBtn("Quit")       ?? FindBtn("Btn_Quit");

            if (!loadSlotsPanel)
            {
                var t = transform.Find("LoadSlotsPanel");
                if (t) loadSlotsPanel = t.gameObject;
            }

            if (!loadSlot1Button) loadSlot1Button = FindBtn("Charger Slot 1") ?? FindBtn("Btn_Load_Slot1");
            if (!loadSlot2Button) loadSlot2Button = FindBtn("Charger Slot 2") ?? FindBtn("Btn_Load_Slot2");
            if (!loadSlot3Button) loadSlot3Button = FindBtn("Charger Slot 3") ?? FindBtn("Btn_Load_Slot3");
            if (!backFromSlotsButton) backFromSlotsButton = FindBtn("Retour") ?? FindBtn("Btn_Back");
        }

        void OnClickNewGame()
        {
            Debug.Log("[MainMenuUI] → New Game");
            PendingLoad.Slot = null; // on démarre vierge
            SceneManager.LoadScene(gameSceneName);
        }

        void OnClickLoadGame()
        {
            Debug.Log("[MainMenuUI] → Load (affiche les slots ou fallback slot1)");
            if (loadSlotsPanel) loadSlotsPanel.SetActive(true);
            else                OnClickLoadSlot("slot1"); // fallback si pas de panel
        }

        void OnClickLoadSlot(string slot)
        {
            Debug.Log($"[MainMenuUI] → Load slot '{slot}'");
            PendingLoad.Slot = slot;
            SceneManager.LoadScene(gameSceneName);
        }

        void HideSlotsPanel()
        {
            if (loadSlotsPanel) loadSlotsPanel.SetActive(false);
        }

        void OnClickQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
