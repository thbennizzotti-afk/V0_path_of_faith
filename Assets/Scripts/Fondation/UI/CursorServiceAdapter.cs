using UnityEngine;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.Fondation.UI
{
    /// <summary>
    /// Adapter qui expose ICursorService et redirige vers CursorService concret,
    /// puis s'enregistre dans le ServiceLocator au démarrage.
    /// </summary>
    [DisallowMultipleComponent]
    public class CursorServiceAdapter : MonoBehaviour, ICursorService
    {
        [SerializeField] private CursorService cursorService;

        private void Awake()
        {
            if (!cursorService)
                cursorService = GetComponent<CursorService>(); // si les 2 composants sont sur le même GO

            // Enregistre CET adapter comme implémentation d'ICursorService
            ServiceLocator.Register<ICursorService>(this);
        }

        public void SetDefault()
        {
            if (cursorService) cursorService.SetBase(CursorType.Default);
        }

        public void SetExploreMove()
        {
            if (cursorService) cursorService.SetBase(CursorType.ExploreMove);
        }
    }
}
