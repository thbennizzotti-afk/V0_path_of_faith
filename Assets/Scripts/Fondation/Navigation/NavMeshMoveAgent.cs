using UnityEngine;
using UnityEngine.AI;

namespace PathOfFaith.Fondation.Navigation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMoveAgent : MonoBehaviour, IMoveAgent
    {
        [Header("Anti-blocage")]
        [Tooltip("Distance minimale considérée comme 'mouvement' (m).")]
        public float stuckDistanceThreshold = 0.05f;

        [Tooltip("Temps (s) sans mouvement avant de considérer l’agent bloqué.")]
        public float stuckTimeThreshold = 1.0f;

        private NavMeshAgent _agent;
        private Vector3 _lastPos;
        private float _stuckTimer;

        public bool HasPath => _agent.hasPath || _agent.pathPending;

        public bool ReachedDestination
        {
            get
            {
                if (_agent.pathPending) return false;
                if (_agent.remainingDistance > _agent.stoppingDistance) return false;
                // Arrivé si pas de path actif OU vitesse très faible
                return !_agent.hasPath || _agent.velocity.sqrMagnitude < 0.01f;
            }
        }

        public bool IsStuck { get; private set; }

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _lastPos = transform.position;

            // Valeurs conseillées (tu peux les mettre aussi dans l’Inspector)
            if (_agent.radius < 0.35f) _agent.radius = 0.4f;
            if (_agent.height < 1.8f) _agent.height = 2.0f;
            if (_agent.angularSpeed < 360f) _agent.angularSpeed = 360f;
            if (_agent.acceleration < 12f) _agent.acceleration = 12f;
            if (_agent.speed < 5.5f) _agent.speed = 5.5f;
            if (_agent.stoppingDistance < 0.2f) _agent.stoppingDistance = 0.25f;

            // Capsule de 2m : base offset ≈ 1 (au besoin ajuste visuellement)
            if (_agent.baseOffset == 0f) _agent.baseOffset = 1f;
        }

        void Update()
        {
            // Heuristique de blocage simple : si on ne bouge pas assez pendant X secondes.
            float moved = (transform.position - _lastPos).magnitude;
            if (moved < stuckDistanceThreshold && HasPath)
                _stuckTimer += Time.deltaTime;
            else
                _stuckTimer = 0f;

            IsStuck = _stuckTimer >= stuckTimeThreshold;
            _lastPos = transform.position;

            // Auto-correct léger : si bloqué, on stoppe pour rendre la main au contrôleur.
            if (IsStuck)
                _agent.ResetPath();
        }

        public bool SetDestination(Vector3 worldPoint)
        {
            if (!_agent.isOnNavMesh) return false;
            bool ok = _agent.SetDestination(worldPoint);
            if (!ok) _agent.ResetPath();
            return ok;
        }

        public void Stop()
        {
            if (_agent == null) return;
            _agent.ResetPath();
        }
    }
}
