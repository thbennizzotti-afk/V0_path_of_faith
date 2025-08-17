using UnityEngine;

namespace PathOfFaith.Fondation.Navigation
{
    /// <summary>
    /// Abstraction du déplacement, pour ne pas dépendre directement de NavMeshAgent.
    /// </summary>
    public interface IMoveAgent
    {
        /// <returns>true si un chemin a été accepté par l’agent</returns>
        bool SetDestination(Vector3 worldPoint);

        /// <summary>Arrête l’agent (clear path).</summary>
        void Stop();

        /// <summary>L’agent a une route en cours ?</summary>
        bool HasPath { get; }

        /// <summary>La destination est atteinte (ou quasi) ?</summary>
        bool ReachedDestination { get; }

        /// <summary>L’agent semble bloqué ? (heuristique simple)</summary>
        bool IsStuck { get; }
    }
}
