using UnityEngine;
using PathOfFaith.Fondation.Navigation;

namespace PathOfFaith.Fondation.Core
{
    // Exécuté très tôt pour enregistrer les services "de base"
    [DefaultExecutionOrder(-5000)]
    public class Bootstrapper : MonoBehaviour
    {
        private static Bootstrapper _instance;

        [Header("Références à enregistrer")]
        public NavigationService navigationService;
        public GameStateManager gameState;

        private void Awake()
        {
            // Singleton anti-doublon
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Persiste à travers les changements de scène
            DontDestroyOnLoad(gameObject);

            // Résolution des références si non assignées dans l'Inspector
#if UNITY_2023_1_OR_NEWER
            if (navigationService == null) navigationService = FindFirstObjectByType<NavigationService>();
            if (gameState == null)        gameState        = FindFirstObjectByType<GameStateManager>();
#else
            if (navigationService == null) navigationService = FindObjectOfType<NavigationService>();
            if (gameState == null)        gameState        = FindObjectOfType<GameStateManager>();
#endif

            // Enregistrement dans le ServiceLocator
            if (navigationService != null) ServiceLocator.Register(navigationService);
            if (gameState != null)         ServiceLocator.Register(gameState);

            // TODO: enregistre ici d'autres services de base si besoin
            // ex: ServiceLocator.Register<ISaveService>(new SaveService());
            // ex: EventBus.Initialize();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null; // utile si "Disable Domain Reload" est activé
        }
    }
}
