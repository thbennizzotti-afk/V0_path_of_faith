// Assets/Scripts/Fondation/Save/JsonSaveService.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PathOfFaith.Save
{
    public class JsonSaveService
    {
        public string SavesDir => Path.Combine(Application.persistentDataPath, "saves");
        private string GetPath(string slot) => Path.Combine(SavesDir, $"{slot}.json");

        public void Save(string slot, GameSave data)
        {
            if (!Directory.Exists(SavesDir)) Directory.CreateDirectory(SavesDir);
            data.savedAtTicks = DateTime.UtcNow.Ticks;

            var path = GetPath(slot);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log($"[Save] Wrote {path}");
        }

        public bool TryLoad(string slot, out GameSave data)
        {
            var path = GetPath(slot);
            if (!File.Exists(path))
            {
                data = null;
                Debug.LogWarning($"[Save] No file at {path}");
                return false;
            }

            var json = File.ReadAllText(path);
            data = JsonUtility.FromJson<GameSave>(json);
            return data != null;
        }

        public bool Exists(string slot) => File.Exists(GetPath(slot));
        public void Delete(string slot) { var p = GetPath(slot); if (File.Exists(p)) File.Delete(p); }

        public IEnumerable<string> ListExistingSlots()
        {
            if (!Directory.Exists(SavesDir)) yield break;
            foreach (var file in Directory.GetFiles(SavesDir, "*.json"))
                yield return Path.GetFileNameWithoutExtension(file);
        }

        public (bool ok, DateTime savedAt, string scene, int version) GetInfo(string slot)
        {
            if (!TryLoad(slot, out var s)) return (false, default, null, 0);
            return (true, new DateTime(s.savedAtTicks, DateTimeKind.Utc), s.scene, s.version);
        }
    }
}
