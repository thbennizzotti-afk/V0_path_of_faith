using UnityEngine;
using UnityEngine.AI;

namespace PathOfFaith.Fondation.Navigation
{
    public interface INavigationService
    {
        PlayerMoveSettings Settings { get; }
        LayerMask GroundMask { get; }
        float MaxRayDistance { get; }
        float SampleMaxDistance { get; }

        void ApplyTo(NavMeshAgent agent);
        bool TryProjectOnNavMesh(Vector3 world, out Vector3 projected);
        bool TryGetClickNavPoint(out Vector3 navPoint);
    }
}
