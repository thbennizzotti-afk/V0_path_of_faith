using UnityEngine;
using UnityEngine.SceneManagement;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.UI
{
    /// <summary>Actions du menu Pause.</summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] PauseManager pauseManager; // assigne l'objet UI_Pause ici (ou sera auto-retrouvé)

        [Header("Options")]
        [SerializeField] string mainMenuScene = "MainMenu";
        [SerializeField] bool autoSaveOnReturnToMainMenu = true;
        [SerializeField] bool autoSaveOnQuitToDesktop   = true;

        void Reset()
        {
            if (!pauseManager) pauseManager = FindFirstObjectByType<PauseManager>();
        }

        // --- Boutons ---
        public void OnClickSave()
        {
            ServiceLocator.Get<ISaveService>().Save(SaveContext.CurrentSlot);
#if UNITY_EDITOR
            Debug.Log($"[PauseMenu] Saved to {SaveContext.CurrentSlot}");
#endif
        }

        public void OnClickResume()
        {
            if (!pauseManager) pauseManager = FindFirstObjectByType<PauseManager>();
            pauseManager?.Resume();
        }

        // Quitter vers le menu principal (recharge la scène MainMenu)
        public void OnClickReturnToMainMenu()
        {
            Time.timeScale = 1f;
            if (autoSaveOnReturnToMainMenu)
                ServiceLocator.Get<ISaveService>().Save(SaveContext.CurrentSlot);

            SceneManager.LoadScene(mainMenuScene);
        }

        // Quitter l’application (ou arrêter le Play Mode dans l’éditeur)
        public void OnClickQuitToDesktop()
        {
            Time.timeScale = 1f;
            if (autoSaveOnQuitToDesktop)
                ServiceLocator.Get<ISaveService>().Save(SaveContext.CurrentSlot);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
