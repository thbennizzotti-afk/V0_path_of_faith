using UnityEngine;
using UnityEngine.AI;

namespace PathOfFaith.Fondation.Navigation
{
    public class NavigationService : MonoBehaviour
    {
        [Header("Raycast sol")]
        public LayerMask groundMask = ~0;
        public float maxRayDistance = 500f;

        [Header("NavMesh sample")]
        public float sampleMaxDistance = 2f;

        [Header("Caméra (optionnel)")]
        [SerializeField] private Camera cameraOverride;

        [Header("Debug")]
        public bool debugLogs = true;   // ← active

        private Camera _cam;

        private void Awake()
        {
            _cam = cameraOverride != null ? cameraOverride : Camera.main;
            if (_cam == null) _cam = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);

            if (debugLogs)
            {
                Debug.Log($"[NavigationService] Camera: {(_cam ? _cam.name : "NULL")}");
                Debug.Log($"[NavigationService] groundMask: {groundMask.value}, sampleMaxDistance: {sampleMaxDistance}");
            }
        }

        public bool TryGetClickNavPoint(out Vector3 navPoint)
        {
            navPoint = default;

            if (_cam == null)
            {
                if (debugLogs) Debug.LogWarning("[NavigationService] Pas de caméra.");
                return false;
            }

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, maxRayDistance, groundMask))
            {
                if (debugLogs) Debug.Log("[NavigationService] Raycast raté (layer/distance).");
                return false;
            }

            if (NavMesh.SamplePosition(hit.point, out var nHit, sampleMaxDistance, NavMesh.AllAreas))
            {
                navPoint = nHit.position;
                if (debugLogs) Debug.Log($"[NavigationService] NavPoint OK: {navPoint}");
                return true;
            }

            if (debugLogs) Debug.Log("[NavigationService] SamplePosition a échoué (NavMesh ? distance ?).");
            return false;
        }
    }
}
