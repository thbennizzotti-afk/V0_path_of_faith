using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Gameplay.Stats
{
    [Serializable]
    public struct StatModifier
    {
        public ModType type;
        public float value;
        public int order;    // ordre d’application
        public object source; // pour RemoveBySource

        public StatModifier(ModType type, float value, int order, object source)
        { this.type = type; this.value = value; this.order = order; this.source = source; }
    }

    /// Valeur d’une stat avec base + liste de modificateurs + cache
    [Serializable]
    public class StatValue
    {
        public float Base;                       // assignée par le builder (template+formules)
        readonly List<StatModifier> _mods = new();
        bool _dirty = true;
        float _cached;

        public StatValue(float baseValue) { Base = baseValue; }

        public void SetBase(float v) { if (!Mathf.Approximately(Base, v)) { Base = v; _dirty = true; } }

        public void AddModifier(StatModifier mod) { _mods.Add(mod); _dirty = true; }
        public void RemoveAllFromSource(object source)
        {
            if (source == null) return;
            _mods.RemoveAll(m => ReferenceEquals(m.source, source));
            _dirty = true;
        }

        public float Get()
        {
            if (!_dirty) return _cached;

            _mods.Sort((a,b) => a.order.CompareTo(b.order));

            float result = Base;
            float percentAddAccumulator = 0f;

            foreach (var m in _mods)
            {
                switch (m.type)
                {
                    case ModType.Flat:       result += m.value; break;
                    case ModType.PercentAdd: percentAddAccumulator += m.value; break;
                    case ModType.PercentMul: result *= (1f + m.value); break;
                }
            }
            if (!Mathf.Approximately(percentAddAccumulator, 0f))
                result *= (1f + percentAddAccumulator);

            _cached = result;
            _dirty = false;
            return _cached;
        }
    }
}
