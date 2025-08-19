using UnityEngine;

namespace PathOfFaith.UI
{
    /// <summary>Gère l'ouverture du menu Pause avec Échap.</summary>
    public class PauseManager : MonoBehaviour
    {
        public GameObject pauseMenuCanvas; // assigner un Canvas/Panel désactivé par défaut
        bool _paused;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
        }

        public void Toggle()
        {
            _paused = !_paused;
            Time.timeScale = _paused ? 0f : 1f;
            if (pauseMenuCanvas) pauseMenuCanvas.SetActive(_paused);
        }

        public void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            if (pauseMenuCanvas) pauseMenuCanvas.SetActive(false);
        }
    }
}
