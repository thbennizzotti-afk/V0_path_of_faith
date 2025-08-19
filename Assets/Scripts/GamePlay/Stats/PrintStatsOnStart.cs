using UnityEngine;
using PathOfFaith.Gameplay.Stats;

public class PrintStatsOnStart : MonoBehaviour
{
    [SerializeField] CharacterStats stats;

    void Start()
    {
        if (!stats) stats = GetComponent<CharacterStats>();
        if (!stats) return;

        int level = GetInt(stats, "Level");
        int hpMax = Mathf.RoundToInt(StatsPreview.Eval(stats, StatType.HPMax, null));
        int apMax = Mathf.RoundToInt(StatsPreview.Eval(stats, StatType.APMax, null));
        int mpMax = Mathf.RoundToInt(StatsPreview.Eval(stats, StatType.MPMax, null));
        int atk   = Mathf.RoundToInt(StatsPreview.Eval(stats, StatType.AttackPower, null));
        int def   = Mathf.RoundToInt(StatsPreview.Eval(stats, StatType.Defense, null));
        int init  = Mathf.RoundToInt(StatsPreview.Eval(stats, StatType.Initiative, null));

        Debug.Log($"[Stats] Lvl={level} HPMax={hpMax} APMax={apMax} MPMax={mpMax} ATK={atk} DEF={def} INIT={init}");
    }

    static int GetInt(object o, string name, int fallback = 0)
    {
        if (o == null) return fallback;
        var t = o.GetType();
        var p = t.GetProperty(name, System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.IgnoreCase);
        if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(o);
        var f = t.GetField(name, System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.IgnoreCase);
        if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(o);
        return fallback;
    }
}
