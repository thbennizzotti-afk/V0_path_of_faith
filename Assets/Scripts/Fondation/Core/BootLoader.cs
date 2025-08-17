using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathOfFaith.Fondation.Core
{
    public enum BootNextScene { MainMenu, Game }
    public enum BootMode { Auto, PromptInEditor }  // << nouveau

    [DisallowMultipleComponent]
    public class BootLoader : MonoBehaviour
    {
        [Header("Mode de démarrage")]
        [SerializeField] BootMode mode = BootMode.PromptInEditor;  // << mets PromptInEditor pour avoir le choix
        [SerializeField] BootNextScene next = BootNextScene.MainMenu; // utilisé en mode Auto

        [Header("Noms exacts des scènes (Build Settings)")]
        public string mainMenuSceneName = "MainMenu";
        public string gameSceneName = "Game";

        bool _showPrompt;     // affiche l’UI de choix
        bool _isLoading;      // évite double-clic

        void Start()
        {
            // En build final non-débug : jamais de prompt (même si tu as laissé PromptInEditor)
            bool allowPrompt = Application.isEditor || Debug.isDebugBuild;

            if (mode == BootMode.PromptInEditor && allowPrompt)
            {
                _showPrompt = true; // on attend ton choix (UI ci-dessous)
                return;
            }

            // Mode Auto : on enchaîne directement
            string target = (next == BootNextScene.MainMenu) ? mainMenuSceneName : gameSceneName;
            StartCoroutine(LoadAndSwitch(target));
        }

        System.Collections.IEnumerator LoadAndSwitch(string sceneName)
        {
            if (_isLoading) yield break;
            _isLoading = true;

            // Laisse 1 frame pour que le Bootstrapper initialise ses services
            yield return null;

            // Basic safety
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[BootLoader] Nom de scène vide.");
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError("[BootLoader] Echec de chargement: " + sceneName + " (vérifie Scenes In Build)");
                yield break;
            }
            while (!op.isDone) yield return null;
        }

        // Petite UI immédiate (IMGUI) pour choisir MainMenu / Game quand on est en Editor/DevBuild
        void OnGUI()
        {
            if (!_showPrompt || _isLoading) return;

            const int w = 260, h = 150;
            var rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            GUILayout.BeginArea(rect, "Boot – Choisir la scène", GUI.skin.window);
            GUILayout.Label("Que veux-tu charger ?");

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Main Menu (M)"))
                {
                    _showPrompt = false;
                    StartCoroutine(LoadAndSwitch(mainMenuSceneName));
                }
                if (GUILayout.Button("Game (G)"))
                {
                    _showPrompt = false;
                    StartCoroutine(LoadAndSwitch(gameSceneName));
                }
            }

            GUILayout.Space(6);
            // Raccourcis clavier
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.M)
                {
                    _showPrompt = false;
                    StartCoroutine(LoadAndSwitch(mainMenuSceneName));
                    e.Use();
                }
                else if (e.keyCode == KeyCode.G)
                {
                    _showPrompt = false;
                    StartCoroutine(LoadAndSwitch(gameSceneName));
                    e.Use();
                }
            }

            // Bouton de bascule rapide vers Auto (main menu)
            if (GUILayout.Button("Toujours Auto → MainMenu"))
            {
                mode = BootMode.Auto;
                next = BootNextScene.MainMenu;
                _showPrompt = false;
                StartCoroutine(LoadAndSwitch(mainMenuSceneName));
            }

            GUILayout.EndArea();
        }
    }
}
