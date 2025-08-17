using UnityEngine;

namespace PathOfFaith.Fondation.Core
{
    [CreateAssetMenu(fileName = "ExplorationConfig", menuName = "PathOfFaith/Exploration Config")]
    public class ExplorationConfig : ScriptableObject
    {
        [Header("Camera Follow")]
        [Range(15f, 75f)] public float pitch = 45f;
        public float baseDistance = 12f;
        public float minDistance = 3f;
        public float maxDistance = 30f;
        public float zoomSpeedFollow = 10f;
        public float orbitSpeed = 220f;
        public float followLerp = 12f;

        [Header("Camera Free")]
        public float freeMoveSpeed = 20f;
        public float freeZoomSpeed = 20f;
        public float freeStartHeight = 30f;

        [Header("Transitions")]
        public float transitionSpeed = 12f;
        public bool snapRotationOnFree = true;

        [Header("Input thresholds")]
        public float orbitHoldThreshold = 0.15f;
        public float scrollDeadzone = 0.05f;
        public float orbitDeadzone = 0.02f;
        public bool invertHorizontal = false;
        public bool invertVertical = false;

        [Header("Click-to-move (Player)")]
        public float playerMoveSpeed = 5f;
        public float clickHoldThreshold = 0.15f;
        public float dragPixelThreshold = 6f;

        [Header("Navigation Service")]
        [Min(10f)] public float rayMaxDistance = 1000f;
        public bool useNavMeshSample = true;
        [Min(0.1f)] public float sampleMaxDistance = 8f;
        public int areaMask = -1; // NavMesh.AllAreas
        public bool debugRays = false;
    }
}
