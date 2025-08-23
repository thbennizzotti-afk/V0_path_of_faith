// Assets/Scripts/Fondation/Save/SaveManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Save
{
    /// <summary>
    /// Capture / charge / applique les sauvegardes.
    /// Ne change PAS de scène : applique dans la scène courante.
    /// </summary>
    public class SaveManager
    {
        private readonly List<ISaveParticipant> _participants = new();
        private readonly JsonSaveService _io = new();

        // --- file d'attente pour "Load depuis le menu" ---
        private string _pendingSlot;                 // null => rien en attente
        public bool HasPending => !string.IsNullOrEmpty(_pendingSlot);

        /// <summary>Appelé depuis le Main Menu AVANT de charger la scène Game.</summary>
        public void QueueLoadForNextScene(string slot)
        {
            _pendingSlot = slot;
#if UNITY_EDITOR
            Debug.Log($"[Load] Queued slot '{slot}' for next scene.");
#endif
        }

        /// <summary>Appelé au démarrage de la scène Game pour appliquer la save en attente.</summary>
        public void ApplyPendingIfAny()
        {
            if (!HasPending) return;

            var slot = _pendingSlot;
            _pendingSlot = null;

            LoadAndApplyInCurrentScene(slot);
#if UNITY_EDITOR
            Debug.Log($"[Load] Pending slot '{slot}' applied in scene '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'.");
#endif
        }

        // --- Enregistrement des participants ---
        public void Register(ISaveParticipant p)
        {
            if (p != null && !_participants.Contains(p)) _participants.Add(p);
        }

        public void Unregister(ISaveParticipant p)
        {
            if (p != null) _participants.Remove(p);
        }

        // --- Sauvegarde ---
        public void Save(string slot)
        {
            var save = new GameSave
            {
                version = 1,
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                savedAtTicks = DateTime.UtcNow.Ticks
            };

            var list = _participants.Count > 0 ? _participants : FindAllParticipantsInScene();
            foreach (var p in list) p.Capture(save);

            _io.Save(slot, save);
#if UNITY_EDITOR
            Debug.Log($"[Save] Captured {list.Count} participants, scene={save.scene}");
#endif
        }

        // --- Chargement + application dans la scène courante ---
        public bool LoadAndApplyInCurrentScene(string slot)
        {
            if (!_io.TryLoad(slot, out var save)) return false;

            var list = _participants.Count > 0 ? _participants : FindAllParticipantsInScene();
            foreach (var p in list) p.Apply(save);

#if UNITY_EDITOR
            Debug.Log($"[Load] Applied {list.Count} participants in scene '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' (slot {slot})");
#endif
            return true;
        }

        // --- Infos pour l’UI ---
        public bool Exists(string slot) => _io.Exists(slot);
        public (bool ok, DateTime savedAt, string scene, int version) GetInfo(string slot) => _io.GetInfo(slot);

        // --- Scan fallback (si pas de Register) ---
        private static List<ISaveParticipant> FindAllParticipantsInScene()
        {
            var found = new List<ISaveParticipant>();
#if UNITY_2023_1_OR_NEWER
            var mbs = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
#else
            var mbs = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
#endif
            foreach (var mb in mbs) if (mb is ISaveParticipant p) found.Add(p);
            return found;
        }
    }
}
