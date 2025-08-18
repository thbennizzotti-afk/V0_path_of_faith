using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathOfFaith.Gameplay.Stats
{
    public interface IStatReader
    {
        float GetStat(StatType type);
        int Level { get; }
    }

    [DisallowMultipleComponent]
    public class CharacterStats : MonoBehaviour, IStatReader
    {
        [Header("Références")]
        public StatsTemplate template;
        public StatFormulaSet formulas;

        [Header("Progression")]
        [Min(1)] public int level = 1;
        [Min(0)] public int unspentAttributePoints = 5;

        [Serializable]
        public struct Allocation { public PrimaryStat stat; public int points; }
        [Tooltip("Points manuellement alloués par le joueur (sauvegardés)")]
        public List<Allocation> allocated = new();

        [Header("Ressources runtime")]
        public int hpCurrent;
        public int apCurrent;
        public int mpCurrent;

        readonly StatSheet _sheet = new();
        public int Level => level;

        void Awake()
        {
            if (!template || !formulas)
                Debug.LogWarning($"[CharacterStats] {name} n'a pas Template/Formulas assignés.");

            RebuildSheet(); // construit toutes les stats et valeurs max
            FullRestore();  // initialise PV/PA/PM courants
        }

        // ----- Construction des attributs totaux (template + perLevel + allocations) -----
        void ComputePrimary(out int str, out int agi, out int intel, out int vit)
        {
            int levelOffset = level - 1;
            int a(PrimaryStat s, int def) {
                foreach (var e in allocated) if (e.stat == s) return e.points;
                return 0;
            }

            str   = template ? template.strength  + template.strPerLevel  * levelOffset + a(PrimaryStat.Strength,0)  : 5;
            agi   = template ? template.agility   + template.agiPerLevel  * levelOffset + a(PrimaryStat.Agility,0)   : 5;
            intel = template ? template.intellect + template.intPerLevel  * levelOffset + a(PrimaryStat.Intellect,0) : 5;
            vit   = template ? template.vitality  + template.vitPerLevel  * levelOffset + a(PrimaryStat.Vitality,0)  : 5;
        }

        public void RebuildSheet()
        {
            ComputePrimary(out var str, out var agi, out var intel, out var vit);
            _sheet.Build(formulas, level, str, agi, intel, vit);
        }

        public void FullRestore()
        {
            hpCurrent = Mathf.RoundToInt(_sheet.Get(StatType.HPMax));
            apCurrent = Mathf.RoundToInt(_sheet.Get(StatType.APMax));
            mpCurrent = Mathf.RoundToInt(_sheet.Get(StatType.MPMax));
        }

        // ----- API publique -----
        public float GetStat(StatType type) => _sheet.Get(type);

        public void AddModifier(StatType type, StatModifier mod) => _sheet.AddModifier(type, mod);
        public void RemoveModifiersFromSource(object source) => _sheet.RemoveModifiersFromSource(source);

        public bool AllocatePoints(PrimaryStat stat, int points)
        {
            if (points <= 0 || unspentAttributePoints < points) return false;
            int idx = allocated.FindIndex(e => e.stat == stat);
            if (idx >= 0) allocated[idx] = new Allocation { stat = stat, points = allocated[idx].points + points };
            else          allocated.Add(new Allocation { stat = stat, points = points });

            unspentAttributePoints -= points;
            RebuildSheet(); // recalcule toutes les dérivées
            return true;
        }

        public void GainLevel(int levels = 1)
        {
            if (levels <= 0) return;
            level += levels;
            if (template) unspentAttributePoints += template.attributePointsOnLevelUp * levels;
            RebuildSheet();
            FullRestore(); // option : full heal au level up
        }

        // Helpers combat
        public void SpendAP(int v) { apCurrent = Mathf.Max(0, apCurrent - v); }
        public void SpendMP(int v) { mpCurrent = Mathf.Max(0, mpCurrent - v); }
        public void TakeDamage(int dmg) { hpCurrent = Mathf.Max(0, hpCurrent - dmg); }
        public bool IsDead => hpCurrent <= 0;
    }
}
