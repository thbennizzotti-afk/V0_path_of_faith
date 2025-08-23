using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSaveReceiver : MonoBehaviour
{
    [Header("Root du joueur (optionnel)")]
    public Transform playerRoot;

    void Start()
    {
        // Receiver neutre : prêt pour un mapping futur sans casser la compile.
        Debug.Log("[PlayerSaveReceiver] Actif (neutre) – mapping désactivé pour l’instant.");
    }
}
