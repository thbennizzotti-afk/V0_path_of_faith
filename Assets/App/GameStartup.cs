using System.Collections;
using System.IO;
using UnityEngine;
using PathOfFaith.Fondation.Core;   // PendingLoad

[DefaultExecutionOrder(1000)]
public class GameStartup : MonoBehaviour
{
    [Header("Cible à repositionner (PlayerRoot)")]
    [SerializeField] private Transform playerRoot;  // Glisse ici le GO racine du joueur

    private IEnumerator Start()
    {
        // Laisse le temps aux autres systèmes de s'initialiser
        yield return null;

        // Slot demandé depuis le MainMenu ?
        var slot = PendingLoad.Slot;
        if (string.IsNullOrEmpty(slot))
            yield break;

        // Consommation one‑shot
        PendingLoad.Slot = null;

        // Applique la pose depuis le JSON du slot
        TryApplyPlayerPoseFromJson(slot);
    }

    private void TryApplyPlayerPoseFromJson(string slot)
    {
        if (!playerRoot)
        {
            Debug.LogWarning("[GameStartup] playerRoot non assigné → pas d'application de la pose.");
            return;
        }

        var path = Path.Combine(Application.persistentDataPath, "saves", $"{slot}.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[GameStartup] Fichier introuvable: {path}");
            return;
        }

        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<GameSaveDTO>(json);
        if (data == null || data.player == null)
        {
            Debug.LogWarning("[GameStartup] JSON lu mais données player null/invalides.");
            return;
        }

        // Position
        playerRoot.position = new Vector3(data.player.x, data.player.y, data.player.z);

        // Rotation (quaternion)
        var q = new Quaternion(data.player.rx, data.player.ry, data.player.rz, data.player.rw);
        playerRoot.rotation = q;

        Debug.Log($"[GameStartup] Pose appliquée → pos=({data.player.x:F2},{data.player.y:F2},{data.player.z:F2}) rot={q}");
    }

    // --- DTOs qui collent à ton JSON ---
#pragma warning disable 0649 // champs remplis par JsonUtility au runtime
    [System.Serializable]
    private class GameSaveDTO
    {
        public int version;
        public string scene;
        public long savedAtTicks;
        public PlayerDTO player;
    }

    [System.Serializable]
    private class PlayerDTO
    {
        public float x, y, z;
        public float rx, ry, rz, rw;
    }
#pragma warning restore 0649
}
