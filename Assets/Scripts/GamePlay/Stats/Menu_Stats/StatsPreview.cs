using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PathOfFaith.Gameplay.Stats; // adapte si besoin (StatType, CharacterStats, PrimaryStat)

public static class StatsPreview
{
    // ---------- Public API ----------
    public static float Eval(CharacterStats cs, StatType type, Dictionary<PrimaryStat,int> pending)
    {
        if (!cs) return 0f;

        // Bases + pending
        float vit = ReadNumber(cs, new[] { "BaseVitality", "VitalityBase", "VitBase", "Vitality" });
        float str = ReadNumber(cs, new[] { "BaseStrength", "StrengthBase", "StrBase", "Strength" });
        float agi = ReadNumber(cs, new[] { "BaseAgility",  "AgilityBase",  "AgiBase", "Agility" });
        float lvl = ReadNumber(cs, new[] { "Level", "Lvl" });

        if (pending != null)
        {
            if (pending.TryGetValue(PrimaryStat.Vitality, out var p)) vit += p;
            if (pending.TryGetValue(PrimaryStat.Strength, out p))    str += p;
            if (pending.TryGetValue(PrimaryStat.Agility,  out p))    agi += p;
        }

        // 1) Formulas.Evaluate(...) — on essaie différentes signatures
        var formulas = ReadObject(cs, new[] { "Formulas", "Formula", "StatFormulas", "StatFormulaSet" });
        if (formulas != null)
        {
            // candidats de méthodes
            var methodNames = new[] { "Evaluate", "Eval", "Compute", "Calc" };
            foreach (var name in methodNames)
            {
                var methods = formulas.GetType().GetMethods(BF);
                foreach (var mi in methods.Where(m => m.Name == name))
                {
                    var ps = mi.GetParameters();
                    try
                    {
                        object result = null;

                        // (StatType, float, float, float, float level)
                        if (Match(ps, typeof(StatType), typeof(float), typeof(float), typeof(float), typeof(float)))
                            result = mi.Invoke(formulas, new object[] { type, vit, str, agi, lvl });

                        // (StatType, float, float, float)
                        else if (Match(ps, typeof(StatType), typeof(float), typeof(float), typeof(float)))
                            result = mi.Invoke(formulas, new object[] { type, vit, str, agi });

                        // (StatType, CharacterStats, Dictionary<PrimaryStat,int>)
                        else if (Match(ps, typeof(StatType), typeof(CharacterStats), typeof(Dictionary<PrimaryStat,int>)))
                            result = mi.Invoke(formulas, new object[] { type, cs, pending });

                        // (StatType, CharacterStats)
                        else if (Match(ps, typeof(StatType), typeof(CharacterStats)))
                            result = mi.Invoke(formulas, new object[] { type, cs });

                        if (result != null)
                            return UnboxNumber(result);
                    }
                    catch { /* on tente l’autre signature */ }
                }
            }
        }

        // 2) Méthode sur CharacterStats (GetStat/GetValue/Get)
        var csMethods = cs.GetType().GetMethods(BF);
        foreach (var m in csMethods)
        {
            if (m.Name is "GetStat" or "GetValue" or "Get")
            {
                var ps = m.GetParameters();
                try
                {
                    // (StatType)
                    if (Match(ps, typeof(StatType)))
                        return UnboxNumber(m.Invoke(cs, new object[] { type }));

                    // (StatType, Dictionary<PrimaryStat,int>) — peu probable mais on tente
                    if (Match(ps, typeof(StatType), typeof(Dictionary<PrimaryStat,int>)))
                        return UnboxNumber(m.Invoke(cs, new object[] { type, pending }));
                }
                catch { }
            }
        }

        // 3) Lecture directe de champs/propriétés connus (alias multiples)
        switch (type)
        {
            case StatType.HPMax:
                return ReadNumber(cs, new[] { "HPMax", "HpMax", "MaxHP", "MaxHp", "HP_MAX" });
            case StatType.APMax:
                return ReadNumber(cs, new[] { "APMax", "ApMax", "MaxAP", "MaxAp" });
            case StatType.MPMax:
                return ReadNumber(cs, new[] { "MPMax", "MpMax", "MaxMP", "MaxMp" });
            case StatType.AttackPower:
                return ReadNumber(cs, new[] { "AttackPower", "ATK", "Atk", "Attack" });
            case StatType.Defense:
                return ReadNumber(cs, new[] { "Defense", "DEF", "Def" });
            case StatType.Initiative:
                return ReadNumber(cs, new[] { "Initiative", "INIT", "Init" });
            default:
                return 0f;
        }
    }

    // ---------- Helpers réflexion ----------
    static readonly BindingFlags BF =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

    static bool Match(ParameterInfo[] ps, params Type[] types)
    {
        if (ps.Length != types.Length) return false;
        for (int i = 0; i < types.Length; i++)
        {
            var want = types[i];
            var got  = ps[i].ParameterType;

            if (want == typeof(float))
            {
                if (!(got == typeof(float) || got == typeof(double) || got == typeof(int))) return false;
            }
            else if (want == typeof(Dictionary<PrimaryStat,int>))
            {
                if (!got.IsGenericType) return false;
                if (got.GetGenericTypeDefinition() != typeof(Dictionary<,>)) return false;
                var args = got.GetGenericArguments();
                if (args[0] != typeof(PrimaryStat) || args[1] != typeof(int)) return false;
            }
            else
            {
                if (!want.IsAssignableFrom(got)) return false;
            }
        }
        return true;
    }

    static float UnboxNumber(object o)
    {
        if (o is float f32)  return f32;
        if (o is double f64) return (float)f64;
        if (o is int i32)    return i32;
        return 0f;
    }

    static float ReadNumber(object obj, IEnumerable<string> names, float fallback = 0f)
    {
        foreach (var n in names)
        {
            if (TryGetNumber(obj, n, out var v)) return v;
        }
        return fallback;
    }

    static bool TryGetNumber(object obj, string name, out float value)
    {
        value = 0f;
        if (obj == null) return false;
        var t = obj.GetType();

        var prop = t.GetProperty(name, BF);
        if (prop != null)
        {
            var v = prop.GetValue(obj);
            if      (v is int i32)   { value = i32;   return true; }
            else if (v is float f32) { value = f32;   return true; }
            else if (v is double f64){ value = (float)f64; return true; }
        }

        var field = t.GetField(name, BF);
        if (field != null)
        {
            var v = field.GetValue(obj);
            if      (v is int i32f)   { value = i32f;   return true; }
            else if (v is float f32f) { value = f32f;   return true; }
            else if (v is double f64f){ value = (float)f64f; return true; }
        }
        return false;
    }

    static object ReadObject(object obj, IEnumerable<string> names)
    {
        if (obj == null) return null;
        var t = obj.GetType();

        foreach (var n in names)
        {
            var p = t.GetProperty(n, BF);
            if (p != null) return p.GetValue(obj);
            var f = t.GetField(n, BF);
            if (f != null) return f.GetValue(obj);
        }
        return null;
    }
}
