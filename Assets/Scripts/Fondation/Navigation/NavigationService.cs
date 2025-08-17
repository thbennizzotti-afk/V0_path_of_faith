using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
// ⚠️ Ne PAS importer UnityEngine.InputSystem ici. On l'utilise en nom qualifié derrière #if.

namespace PathOfFaith.Fondation.Navigation
{
    public class NavigationService : MonoBehaviour, INavigationService
    {
        [Header("Réglages globaux (ScriptableObject)")]
        [SerializeField] private PlayerMoveSettings settings; // centralisation

        [Header("Fallback si 'settings' est vide (hérités)")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;
        [SerializeField] private float sampleMaxDistance = 2f;

        [Header("Caméra (optionnel)")]
        [SerializeField] private Camera cameraOverride;

        [Header("Debug")]
        public bool debugLogs = true;

        private Camera _cam;

        // === INavigationService ===
        public PlayerMoveSettings Settings => settings;
        public LayerMask GroundMask => settings ? settings.groundMask : groundMask;
        public float MaxRayDistance => settings ? settings.maxRayDistance : maxRayDistance;
        public float SampleMaxDistance => settings ? settings.navmeshSampleMaxDistance : sampleMaxDistance;

        private void Awake()
        {
            _cam = cameraOverride != null ? cameraOverride : Camera.main;
            if (_cam == null) _cam = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);

            if (debugLogs)
            {
                Debug.Log($"[NavigationService] Camera: {(_cam ? _cam.name : "NULL")}", this);
                Debug.Log($"[NavigationService] GroundMask:{GroundMask.value}  Ray:{MaxRayDistance}  Sample:{SampleMaxDistance}", this);
                if (settings) Debug.Log("[NavigationService] Settings asset: " + settings.name, this);
            }

            // Si tu as un ServiceLocator :
            // ServiceLocator.Register<INavigationService>(this);
        }

        /// <summary>Applique les vitesses/rotation à un NavMeshAgent depuis Settings (si présent).</summary>
        public void ApplyTo(NavMeshAgent agent)
        {
            if (!agent || !settings) return;

            agent.speed          = settings.speed;
            agent.acceleration   = settings.acceleration;
            agent.angularSpeed   = settings.angularSpeed;
            agent.updateRotation = settings.agentHandlesRotation;

            if (debugLogs)
                Debug.Log($"[NavigationService] ApplyTo → speed:{agent.speed} accel:{agent.acceleration} ang:{agent.angularSpeed} updateRot:{agent.updateRotation}", this);
        }

        /// <summary>Projette un point monde sur le NavMesh avec la distance de sample configurée.</summary>
        public bool TryProjectOnNavMesh(Vector3 world, out Vector3 projected)
        {
            if (NavMesh.SamplePosition(world, out var nHit, SampleMaxDistance, NavMesh.AllAreas))
            {
                projected = nHit.position;
                return true;
            }
            projected = world;
            return false;
        }

        /// <summary>Depuis un clic écran, retourne une destination valide sur le NavMesh.</summary>
        public bool TryGetClickNavPoint(out Vector3 navPoint)
        {
            navPoint = default;

            // 1) Caméra ?
            var cam = _cam != null ? _cam : Camera.main;
            if (!cam)
            {
                if (debugLogs) Debug.LogWarning("[NavigationService] Aucune caméra trouvée.", this);
                return false;
            }

            // 2) Ignorer l’UI
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return false;

            // 3) Position curseur (NIS ou Legacy)
            Vector3 screenPos;
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                var p = mouse.position.ReadValue();
                screenPos = new Vector3(p.x, p.y, 0f);
            }
            else
            {
                screenPos = Input.mousePosition;
            }
#else
            screenPos = Input.mousePosition;
#endif

            // 4) Raycast sol
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, MaxRayDistance, GroundMask, QueryTriggerInteraction.Ignore))
            {
                if (debugLogs) Debug.Log("[NavigationService] Raycast raté (layer/distance).", this);
                return false;
            }

            // 5) Projection NavMesh
            if (TryProjectOnNavMesh(hit.point, out var projected))
            {
                navPoint = projected;
                if (debugLogs) Debug.Log($"[NavigationService] NavPoint OK: {navPoint}", this);
                return true;
            }

            if (debugLogs) Debug.Log("[NavigationService] SamplePosition a échoué (NavMesh ? distance ?).", this);
            return false;
        }
    }
}
