using UnityEngine;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.App
{
    /// <summary>Applique la sauvegarde au démarrage de la scène Game si demandé.</summary>
    public class GameStartup : MonoBehaviour
    {
        void Start()
        {
            if (!StartOptions.LoadOnStart) return;

            var ok = ServiceLocator.Get<ISaveService>().Load(StartOptions.CurrentSlot);
#if UNITY_EDITOR
            if (!ok) Debug.LogWarning($"[GameStartup] Aucun fichier pour {StartOptions.CurrentSlot}, démarrage en New Game.");
#endif
        }
    }
}
