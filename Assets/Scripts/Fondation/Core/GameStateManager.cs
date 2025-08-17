using System;
using UnityEngine;

namespace PathOfFaith.Fondation.Core
{
    public enum GameState
    {
        Exploration,
        Combat,
        Dialogue,
        Menu
    }

    /// <summary>
    /// Gestionnaire central de l'état du jeu (singleton).
    /// - Survit aux changements de scène.
    /// - Permet de changer l'état proprement (SetState) et notifie via OnStateChanged.
    /// - Offre des helpers IsState(...) et IsExploration().
    /// - Expose un sélecteur d'état dans l'Inspector (Editor State) utilisable en Play.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Inspector")]
        [Tooltip("État sélectionné via l'Inspector (éditable en mode Éditeur et en Play).")]
        [SerializeField] private GameState editorState = GameState.Exploration;

        [Header("Runtime (info)")]
        [SerializeField, ReadOnlyInspector] // attribut facultatif; voir note ci-dessous
        private GameState currentState = GameState.Exploration;
        public GameState CurrentState => currentState;

        /// <summary> Événement déclenché à chaque changement d'état (après mise à jour). </summary>
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // État initial = valeur choisie dans l'Inspector
            currentState = editorState;
        }

        /// <summary>
        /// Change l'état du jeu et notifie les écouteurs si l'état a réellement changé.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            editorState  = newState; // garde l'Inspector synchronisé
            Debug.Log($"[GameStateManager] État → {currentState}");
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Helper de confort : vrai si l'état courant correspond.
        /// </summary>
        public bool IsState(GameState s) => currentState == s;

        /// <summary>
        /// Helper de confort : vrai si en exploration.
        /// </summary>
        public bool IsExploration() => IsState(GameState.Exploration);

        /// <summary>
        /// Synchronise les changements faits via l'Inspector.
        /// - En Play : appliquer vraiment le changement d'état.
        /// - Hors Play : préparer l'état de départ pour la prochaine exécution.
        /// </summary>
        private void OnValidate()
        {
            // Evite d’exécuter du code Unity avant l’initialisation complète
            if (!isActiveAndEnabled) return;

            if (Application.isPlaying)
                SetState(editorState);
            else
                currentState = editorState;
        }
    }

    /// <summary>
    /// Petit attribut pour afficher un champ en lecture seule dans l'Inspector (optionnel).
    /// Si tu n’as pas besoin de ça, tu peux supprimer cette classe et l'attribut au-dessus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }
}
