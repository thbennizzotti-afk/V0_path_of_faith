using UnityEngine;
using UnityEngine.Events;

namespace PathOfFaith.Gameplay.Characters.Player
{
    // Récepteur générique des events d’anim de pas, safe même sans audio.
    public class FootstepEventReceiver : MonoBehaviour
    {
        public enum Foot { Any, Left, Right }

        [System.Serializable] public class FootstepUnityEvent : UnityEvent<Foot> { }

        [Header("Debug")]
        [SerializeField] private bool logFirstEvent = false;
        private bool logged;

        [Header("Sorties (optionnelles)")]
        public FootstepUnityEvent OnFootstepEvent;

        // Les clips peuvent appeler n'importe laquelle de ces signatures :
        public void OnFootstep()                       => Dispatch(Foot.Any);
        public void OnFootstep(AnimationEvent _)       => Dispatch(Foot.Any);
        public void OnFootstepL()                      => Dispatch(Foot.Left);
        public void OnFootstepR()                      => Dispatch(Foot.Right);

        private void Dispatch(Foot foot)
        {
            if (logFirstEvent && !logged)
            {
                Debug.Log($"[Footstep] Event reçu ({foot}) sur {name}", this);
                logged = true;
            }
            OnFootstepEvent?.Invoke(foot); // Rien d’abonné ? -> aucun son, mais pas d’erreur.
        }
    }
}
