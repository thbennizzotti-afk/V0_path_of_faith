using UnityEngine;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Save;

namespace PathOfFaith.App
{
    /// <summary>Installe le SaveManager au boot.</summary>
    public class SaveInstaller : MonoBehaviour
    {
        void Awake()
        {
            var mgr = new SaveManager();
            ServiceLocator.Register<ISaveService>(mgr); // pour l'UI et GameStartup
            ServiceLocator.Register(mgr);               // pour Register/Unregister des participants
        }
    }
}
