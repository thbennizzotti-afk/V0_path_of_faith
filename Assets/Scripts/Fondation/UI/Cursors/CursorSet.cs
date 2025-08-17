using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Fondation.UI
{
    public enum CursorType
    {
        Default,        // curseur générique (exploration + UI par défaut)
        ExploreMove,    // déplacement en exploration
        CombatMove,     // déplacement en combat
        Attack          // mode attaque (petite épée)
    }

    [Serializable]
    public struct CursorEntry
    {
        public CursorType type;
        public Texture2D texture;
        public Vector2 hotspot;         // pixel offset
        public CursorMode mode;         // Auto la plupart du temps
    }

    [CreateAssetMenu(fileName = "CursorSet", menuName = "PathOfFaith/Cursor Set", order = 10)]
    public class CursorSet : ScriptableObject
    {
        public List<CursorEntry> entries = new List<CursorEntry>();

        public bool TryGet(CursorType type, out CursorEntry entry)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].type == type)
                {
                    entry = entries[i];
                    return true;
                }
            }
            entry = default;
            return false;
        }
    }
}
