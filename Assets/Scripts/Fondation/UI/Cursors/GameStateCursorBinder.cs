using UnityEngine;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Fondation.UI;

namespace PathOfFaith.Fondation.UI
{
    [DisallowMultipleComponent]
    public class GameStateCursorBinder : MonoBehaviour
    {
        public CursorService cursorService;

        void Awake()
        {
            if (cursorService == null)
                cursorService = FindFirstObjectByType<CursorService>(FindObjectsInactive.Exclude);
        }

        void Update()
        {
            // Si pas de GSM, on reste en Default (mode éditeur/menus)
            if (cursorService == null) return;

            var gsm = GameStateManager.Instance;
            if (gsm == null || gsm.IsState(GameState.Exploration))
                cursorService.SetBase(CursorType.Default);      // UI + déplacement expl.
            else
                cursorService.SetBase(CursorType.CombatMove);   // déplacement combat
        }
    }
}
