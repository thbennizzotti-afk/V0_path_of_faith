using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Gameplay.Stats
{
    /// Dictionnaire de StatValue + (re)construction depuis Template/Formules/Niveau/Allocations
    public class StatSheet
    {
        readonly Dictionary<StatType, StatValue> _stats = new();

        // Snapshot des attributs utilisés pour (re)calcul
        int _level;
        int _str, _agi, _int, _vit;
        StatFormulaSet _formula;

        public void Build(StatFormulaSet formula, int level, int str, int agi, int intel, int vit)
        {
            _formula = formula; _level = level;
            _str = str; _agi = agi; _int = intel; _vit = vit;

            EnsureAllKeys();

            // Fixer les bases via les formules
            foreach (var kv in _stats)
                kv.Value.SetBase(_formula.Eval(kv.Key, _level, _str, _agi, _int, _vit));
        }

        void EnsureAllKeys()
        {
            // Ajoute une entrée pour chaque StatType utilisé
            void Need(StatType t) { if (!_stats.ContainsKey(t)) _stats[t] = new StatValue(0f); }

            Need(StatType.HPMax); Need(StatType.APMax); Need(StatType.MPMax);
            Need(StatType.AttackPower); Need(StatType.Defense);
            Need(StatType.Initiative); Need(StatType.CritChance); Need(StatType.CritDamageMultiplier);
            Need(StatType.ActionPointsPerTurn); Need(StatType.MovePointsPerTurn); Need(StatType.MoveRange);

            // exposer primaires (utile HUD/IA)
            Need(StatType.Strength); Need(StatType.Agility); Need(StatType.Intellect); Need(StatType.Vitality);
        }

        public float Get(StatType type) => _stats.TryGetValue(type, out var v) ? v.Get() : 0f;

        public void AddModifier(StatType type, StatModifier mod)
        {
            if (_stats.TryGetValue(type, out var v)) v.AddModifier(mod);
        }

        public void RemoveModifiersFromSource(object source)
        {
            foreach (var v in _stats.Values) v.RemoveAllFromSource(source);
        }
    }
}
