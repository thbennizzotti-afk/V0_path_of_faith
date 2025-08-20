using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
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

            if (navigationService) navigationService.ApplyTo(agent);
        }

        private void Update()
        {
            // En pause → on ignore tous les clics gameplay
            if (Time.timeScale == 0f) return;

            if (!WasLeftClickThisFrame()) return;

            // 1) Ignore si on clique sur de l'UI
            if (IsPointerOverAnyUI()) return;

            if (!navigationService || !agent || !agent.enabled) return;

            // 2) Laisse le service faire raycast + projection NavMesh + masks centralisés
            if (navigationService.TryGetClickNavPoint(out var dest))
            {
                agent.SetDestination(dest);
            }
        }

        // --- Helpers Input ---

#if ENABLE_INPUT_SYSTEM
        private static bool WasLeftClickThisFrame()
            => UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame ?? false;

        private static Vector2 CurrentPointerPosition()
            => UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else
        private static bool WasLeftClickThisFrame() => Input.GetMouseButtonDown(0);
        private static Vector2 CurrentPointerPosition() => Input.mousePosition;
#endif

        // --- Helper UI robustifié ---
        // Utilise EventSystem.IsPointerOverGameObject() ET un RaycastAll manuel (fallback)
        private static readonly List<RaycastResult> _uiHits = new();

        private static bool IsPointerOverAnyUI()
        {
            var es = EventSystem.current;
            if (es == null) return false;

            // Test rapide standard (souvent suffisant)
            if (es.IsPointerOverGameObject()) return true;

            // Fallback robuste : raycast UI explicite à la position souris
            var ped = new PointerEventData(es) { position = CurrentPointerPosition() };
            _uiHits.Clear();
            es.RaycastAll(ped, _uiHits);
            return _uiHits.Count > 0;
        }
    }
}
