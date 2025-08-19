using UnityEngine;
using PathOfFaith.Fondation.Core; // ISaveService + SaveContext

namespace PathOfFaith.UI
{
    /// <summary>Boutons du menu Pause (Sauvegarder/Resume).</summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] PauseManager pauseManager; // assigne dans l’Inspector (ou auto dans Reset)

        void Reset()
        {
            if (!pauseManager) pauseManager = GetComponentInParent<PauseManager>();
        }

        public void OnClickSave()
        {
            // Plus de dépendance à PathOfFaith.App : on lit le slot via Core.SaveContext
            ServiceLocator.Get<ISaveService>().Save(SaveContext.CurrentSlot);
#if UNITY_EDITOR
            Debug.Log($"[PauseMenu] Saved to {SaveContext.CurrentSlot}");
#endif
        }

        public void OnClickResume()
        {
            if (!pauseManager)
                pauseManager = Object.FindFirstObjectByType<PauseManager>(); // API non-dépréciée
            pauseManager?.Resume();
        }
    }
}
