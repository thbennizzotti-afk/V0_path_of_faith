using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PathOfFaith.Fondation.Core;

namespace PathOfFaith.Save
{
    /// <summary>Orchestre la liste des participants et l'I/O JSON.</summary>
    public class SaveManager : ISaveService
    {
        private readonly List<ISaveParticipant> _participants = new();
        private readonly JsonSaveService _io = new();

        // Participants (pour tes composants: Player, Inventaire, etc.)
        public void Register(ISaveParticipant p) { if (!_participants.Contains(p)) _participants.Add(p); }
        public void Unregister(ISaveParticipant p) { _participants.Remove(p); }

        // === ISaveService ===
        public void Save(string slot)
        {
            var s = new GameSave { scene = SceneManager.GetActiveScene().name };
            foreach (var p in _participants) p.Capture(s);
            _io.Save(slot, s);
        }

        public bool Load(string slot)
        {
            if (!_io.TryLoad(slot, out var s)) return false;
            foreach (var p in _participants) p.Apply(s);
            return true;
        }

        public bool Exists(string slot) => _io.Exists(slot);
        public void Delete(string slot) => _io.Delete(slot);
        public IEnumerable<string> ListExistingSlots() => _io.ListExistingSlots();
        public (bool ok, DateTime savedAt, string scene, int version) GetInfo(string slot) => _io.GetInfo(slot);
    }
}
