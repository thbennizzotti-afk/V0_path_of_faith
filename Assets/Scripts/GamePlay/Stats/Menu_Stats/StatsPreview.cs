using System.Collections.Generic;
using UnityEngine;
using PathOfFaith.Gameplay.Stats;

public static class StatsPreview
{
    public static void ComputePrimaries(
        CharacterStats s,
        Dictionary<PrimaryStat,int> pending,
        out int STR, out int AGI, out int INT, out int VIT)
    {
        int levelOffset = s.level - 1;

        int Alloc(PrimaryStat stat)
        {
            int cur = 0;
            foreach (var a in s.allocated)
                if (a.stat == stat) { cur = a.points; break; }

            int pend = 0;                       // << clé : initialiser
            if (pending != null)
                pending.TryGetValue(stat, out pend);

            return cur + pend;
        }

        STR = (s.template ? s.template.strength  + s.template.strPerLevel  * levelOffset : 5) + Alloc(PrimaryStat.Strength);
        AGI = (s.template ? s.template.agility   + s.template.agiPerLevel  * levelOffset : 5) + Alloc(PrimaryStat.Agility);
        INT = (s.template ? s.template.intellect + s.template.intPerLevel  * levelOffset : 5) + Alloc(PrimaryStat.Intellect);
        VIT = (s.template ? s.template.vitality  + s.template.vitPerLevel  * levelOffset : 5) + Alloc(PrimaryStat.Vitality);
    }

    public static float Eval(CharacterStats s, StatType type, Dictionary<PrimaryStat,int> pending)
    {
        if (s == null || s.formulas == null) return 0f;
        ComputePrimaries(s, pending, out var STR, out var AGI, out var INT, out var VIT);
        return s.formulas.Eval(type, s.level, STR, AGI, INT, VIT);
    }
}
