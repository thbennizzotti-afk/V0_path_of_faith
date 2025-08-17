using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using PathOfFaith.Fondation.Navigation;

namespace PathOfFaith.Gameplay.Characters.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerClickToMove : MonoBehaviour
    {
        [Header("Réfs")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private NavigationService navigationService; // auto-résolu si null

        private void Reset()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Awake()
        {
            if (!agent) agent = GetComponent<NavMeshAgent>();

            // Résolution du service (ou via ton ServiceLocator si tu préfères)
            if (!navigationService)
#if UNITY_2023_1_OR_NEWER
                navigationService = FindFirstObjectByType<NavigationService>();
#else
                navigationService = FindObjectOfType<NavigationService>();
#endif

            // Appliquer les vitesses/rotations de l’asset sur l’agent
            if (navigationService) navigationService.ApplyTo(agent);
        }

        private void Update()
        {
            if (!WasLeftClickThisFrame()) return;

            // Ignorer l’UI
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!navigationService) return;

            // Laisse le service faire raycast + projection NavMesh + masks centralisés
            if (navigationService.TryGetClickNavPoint(out var dest))
            {
                agent.SetDestination(dest);
            }
        }

#if ENABLE_INPUT_SYSTEM
        private static bool WasLeftClickThisFrame()
            => UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame ?? false;
#else
        private static bool WasLeftClickThisFrame() => Input.GetMouseButtonDown(0);
#endif
    }
}
