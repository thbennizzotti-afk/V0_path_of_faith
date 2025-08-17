using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Fondation.UI
{
    [DisallowMultipleComponent]
    public class CursorService : MonoBehaviour
    {
        [Header("Set de curseurs")]
        public CursorSet cursorSet;

        [Header("Base quand aucun override n'est actif")]
        public CursorType baseCursor = CursorType.Default;

        [Header("Debug")]
        public bool debugLogs = false;

        private struct StackItem
        {
            public int id;
            public int priority;
            public CursorType type;
        }

        private readonly List<StackItem> _stack = new();
        private int _nextId = 1;
        private CursorType _currentApplied;
        private bool _initialized;

        private void OnEnable()
        {
            Apply(baseCursor);
            _initialized = true;
        }

        /// <summary>Change le curseur "de base". Utilisé quand la pile est vide.</summary>
        public void SetBase(CursorType type)
        {
            baseCursor = type;
            if (_stack.Count == 0) Apply(baseCursor);
        }

        /// <summary>Ajoute un curseur temporaire avec priorité. Retourne un handle à utiliser pour Pop.</summary>
        public int Push(CursorType type, int priority = 0)
        {
            var item = new StackItem { id = _nextId++, priority = priority, type = type };
            _stack.Add(item);
            Recompute();
            return item.id;
        }

        /// <summary>Retire un curseur temporaire par handle.</summary>
        public void Pop(int handle)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i].id == handle)
                {
                    _stack.RemoveAt(i);
                    Recompute();
                    return;
                }
            }
        }

        /// <summary>Applique directement un curseur (utilise la pile avec une priorité donnée).</summary>
        public int OverrideOnce(CursorType type, int priority = 1000)
        {
            // Convenience pour les overrides forts (ex: drag caméra, UI modal, etc.)
            return Push(type, priority);
        }

        /// <summary>Force l’application d’un type (sans passer par la pile). Évite si possible.</summary>
        public void ForceApply(CursorType type)
        {
            Apply(type);
        }

        private void Recompute()
        {
            if (_stack.Count == 0)
            {
                Apply(baseCursor);
                return;
            }

            // prend l'item de priorité max (dernier resort: le plus récent si égalité)
            int bestIndex = 0;
            int bestPriority = _stack[0].priority;
            for (int i = 1; i < _stack.Count; i++)
            {
                if (_stack[i].priority > bestPriority)
                {
                    bestPriority = _stack[i].priority;
                    bestIndex = i;
                }
            }
            Apply(_stack[bestIndex].type);
        }

        private void Apply(CursorType type)
        {
            if (!_initialized && cursorSet == null) return;

            if (_currentApplied.Equals(type)) return;

            if (cursorSet != null && cursorSet.TryGet(type, out var entry) && entry.texture != null)
            {
                Cursor.SetCursor(entry.texture, entry.hotspot, entry.mode);
                if (debugLogs) Debug.Log($"[CursorService] Apply: {type}");
            }
            else
            {
                // fallback OS default
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                if (debugLogs) Debug.Log($"[CursorService] Apply: {type} (fallback)");
            }
            _currentApplied = type;
        }
    }
}
