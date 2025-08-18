using UnityEngine;

namespace PathOfFaith.Gameplay.Stats
{
    [CreateAssetMenu(menuName = "PathOfFaith/Stats/Formula Set", fileName = "StatFormulaSet")]
    public class StatFormulaSet : ScriptableObject
    {
        [Header("PV / PA / PM")]
        public int baseHP = 50;
        public int hpPerVitality = 10;
        public int hpPerLevel = 5;

        public int baseAP = 6;
        public int baseMP = 6;

        [Header("Offense / Défense")]
        public float atkPerStrength = 2.5f;
        public float defPerVitality = 1.5f;

        [Header("Initiative & Critiques")]
        public int baseInitiative = 5;
        public float initPerAgility = 1.0f;
        [Range(0f,0.8f)] public float critPerAgility = 0.0025f; // +0.25% par point d’AGI
        public float baseCritChance = 0.05f;                     // 5%
        public float critDamageMultiplier = 1.5f;                // x1.5

        [Header("Déplacements / Actions")]
        public int actionPointsPerTurn = 6;
        public int movePointsPerTurn = 6;
        public float moveRangeFromAgility = 0.1f; // cases/point (si tu utilises des cases)

        // Évalue une stat dérivée à partir des attributs / niveau
        public float Eval(StatType t, int level, int str, int agi, int intel, int vit)
        {
            return t switch
            {
                StatType.HPMax        => baseHP + vit * hpPerVitality + (level-1) * hpPerLevel,
                StatType.APMax        => baseAP,
                StatType.MPMax        => baseMP,
                StatType.AttackPower  => str * atkPerStrength,
                StatType.Defense      => vit * defPerVitality,
                StatType.Initiative   => baseInitiative + agi * initPerAgility,
                StatType.CritChance   => Mathf.Clamp01(baseCritChance + agi * critPerAgility),
                StatType.CritDamageMultiplier => critDamageMultiplier,
                StatType.ActionPointsPerTurn  => actionPointsPerTurn,
                StatType.MovePointsPerTurn    => movePointsPerTurn,
                StatType.MoveRange            => agi * moveRangeFromAgility,
                // on peut exposer les primaires via les stat types
                StatType.Strength     => str,
                StatType.Agility      => agi,
                StatType.Intellect    => intel,
                StatType.Vitality     => vit,
                _ => 0f
            };
        }
    }
}
