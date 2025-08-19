using UnityEngine;
using UnityEngine.SceneManagement;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.App
{
    /// <summary>Logique minimale des boutons New/Load (slot1).</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] string gameSceneName = "Game";

        public void OnClickNew_Slot1()
        {
            var save = ServiceLocator.Get<ISaveService>();
            if (save.Exists("slot1")) save.Delete("slot1");
            StartOptions.NewGame("slot1");
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnClickLoad_Slot1()
        {
            StartOptions.LoadGame("slot1");
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnClickQuit() => Application.Quit();
    }
}
