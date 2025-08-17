using UnityEngine;

namespace PathOfFaith.Fondation.Navigation
{
    [CreateAssetMenu(menuName = "Game/Config/Player Move Settings", fileName = "PlayerMoveSettings")]
    public class PlayerMoveSettings : ScriptableObject
    {
        [Header("NavMeshAgent")]
        public float speed = 5f;
        public float acceleration = 16f;
        public float angularSpeed = 1080f;

        [Header("Rotation")]
        public bool agentHandlesRotation = false;       // true = rotation gérée par l'agent
        public float manualRotationSpeedDeg = 1440f;    // utilisé si rotation manuelle

        [Header("Click To Move")]
        public LayerMask groundMask = ~0;
        public float maxRayDistance = 500f;
        public float navmeshSampleMaxDistance = 2f;
    }
}
