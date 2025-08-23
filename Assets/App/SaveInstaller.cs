using UnityEngine;

/// <summary>
/// Installer neutre : persiste simplement l'objet au-delà des changements de scène.
/// On n'enregistre plus d'ISaveService ici (GameStartup lit directement le JSON).
/// </summary>
[DisallowMultipleComponent]
public class SaveInstaller : MonoBehaviour
{
    private static bool s_done;

    void Awake()
    {
        if (s_done) { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        s_done = true;
        Debug.Log("[SaveInstaller] Ready (noop, no service registration).");
    }
}
