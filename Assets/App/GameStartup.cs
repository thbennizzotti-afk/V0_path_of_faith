// Assets/App/GameStartup.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using PathOfFaith.App;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Save;

namespace PathOfFaith.App
{
    /// <summary>
    /// Au démarrage de la scène de jeu, applique la sauvegarde si on en a demandé une.
    /// </summary>
    [DefaultExecutionOrder(500)] // après SaveInstaller/ServiceLocator
    public class GameStartup : MonoBehaviour
    {
        void Start()
        {
            if (!StartOptions.HasPendingLoad) return;

            var slot = StartOptions.Consume(); // récupère et efface
            var save = ServiceLocator.Get<ISaveService>();
            var mgr  = ServiceLocator.Get<SaveManager>();

            // On lit les infos du slot (scène cible).
            var info = mgr.GetInfo(slot);
            var current = SceneManager.GetActiveScene().name;

            if (!info.ok)
            {
                Debug.LogWarning($"[GameStartup] Slot '{slot}' introuvable → pas de chargement.");
                return;
            }

            if (info.scene != current)
            {
                // On charge d’abord la bonne scène, puis on applique la save à l’arrivée.
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene(info.scene);
            }
            else
            {
                // Déjà sur la bonne scène → applique maintenant.
                save.Load(slot);
            }

            void OnSceneLoaded(Scene s, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                save.Load(slot);
            }
        }
    }
}
