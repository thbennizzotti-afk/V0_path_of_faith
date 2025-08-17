using UnityEngine;
using UnityEngine.AI;

namespace PathOfFaith.Gameplay.Characters.Player
{
    /// <summary>
    /// Alimente les paramètres Animator à partir de la vitesse du NavMeshAgent.
    /// Option "normalizeToAgentSpeed" pour utiliser un BlendTree 0..1.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        [Header("Références")]
        [SerializeField] private Animator animator;      // -> Animator du 'Visual'
        [SerializeField] private NavMeshAgent agent;     // -> NavMeshAgent du root

        [Header("Réglages")]
        [Tooltip("Seuil au-dessus duquel on considère que le joueur bouge.")]
        [SerializeField] private float movingThreshold = 0.1f;

        [Tooltip("Si vrai, envoie Speed normalisé 0..1 (v/agent.speed). Sinon, envoie la vitesse en m/s.")]
        [SerializeField] private bool normalizeToAgentSpeed = false;

        private static readonly int HashSpeed = Animator.StringToHash("Speed");
        private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");

        private void Reset()
        {
            agent = GetComponent<NavMeshAgent>();
            if (!animator) animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            if (!agent) agent = GetComponent<NavMeshAgent>();
            if (!animator) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (!animator || !agent) return;

            float v = agent.velocity.magnitude; // m/s
            float speedParam;

            if (normalizeToAgentSpeed)
            {
                float denom = Mathf.Max(0.01f, agent.speed);
                speedParam = Mathf.Clamp01(v / denom); // 0..1
            }
            else
            {
                speedParam = v; // m/s
            }

            animator.SetFloat(HashSpeed, speedParam);
            animator.SetBool(HashIsMoving, v > movingThreshold);
        }
    }
}
