using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using PathOfFaith.Gameplay.Stats;

public class PartyStatsMenuUI : MonoBehaviour
{
    [Header("Références")]
    public UnityEngine.Object party;              // optionnel (PartyService)
    public Transform columnsContainer;            // Grid Layout Group
    public CharacterSheetColumnUI columnPrefab;   // prefab colonne (TMP)
    public Button validateButton;
    public Button cancelButton;
    public Button closeButton;

    [Header("Options")]
    [Range(1, 8)] public int maxColumns = 4;
    public bool keepHpRatioOnValidate = true;

    readonly List<CharacterSheetColumnUI> _columns = new();
    readonly List<CharacterStats> _members = new();
    readonly Dictionary<CharacterStats, Dictionary<PrimaryStat, int>> _pending = new();

    // Aliases minimaux (ceux qui ont déjà matché chez toi dans les logs)
    static readonly string[] ALIAS_UNSPENT = { "UnspentAttribute","Unspent","UnspentPoints","UnspentAttributes","StatPoints","AttributePoints","PointsToSpend" };
    static readonly string[] ALIAS_ALLOCATED = { "Allocated","Allocations","AllocatedStats","AllocatedAttributes" };
    static readonly string[] ALIAS_HP_MAX   = { "HPMax","HpMax","MaxHP","MaxHp" };
    static readonly string[] ALIAS_HP_CUR   = { "HPCurrent","HpCurrent","CurrentHP","Hp","HP" };

    // Méthodes de recalcul les plus probables
    static readonly string[] RECALC = {
        "RecalculateAll","Recalculate","RebuildStats","RefreshStats",
        "UpdateDerived","UpdateAll","ComputeStats","ApplyStats","Refresh"
    };

    const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

    void Awake()
    {
        if (validateButton) validateButton.onClick.AddListener(OnValidate);
        if (cancelButton)   cancelButton.onClick.AddListener(OnCancel);
        if (closeButton)    closeButton.onClick.AddListener(Hide);
    }

    void OnEnable()
    {
        BuildColumns();
        RefreshAll();
    }

    // ---------- UI ----------
    void BuildColumns()
    {
        if (!columnsContainer || !columnPrefab)
        {
            Debug.LogError("[PartyStatsMenuUI] columnsContainer/columnPrefab manquants.");
            return;
        }

        foreach (Transform c in columnsContainer) Destroy(c.gameObject);
        _columns.Clear();
        _members.Clear();

        CollectMembers(_members);

        int count = Mathf.Min(_members.Count, maxColumns);
        for (int i = 0; i < count; i++)
        {
            var cs = _members[i];
            var col = Instantiate(columnPrefab, columnsContainer);
            col.Setup(cs, GetPendingFor, GetRemainingPoints, RequestAddPoint);
            _columns.Add(col);
        }
        for (int i = count; i < maxColumns; i++)
        {
            var col = Instantiate(columnPrefab, columnsContainer);
            col.SetupPlaceholder($"vide {i + 1}");
            _columns.Add(col);
        }
    }

    void CollectMembers(List<CharacterStats> dst)
    {
        dst.Clear();

        if (party != null)
        {
            try
            {
                var t = party.GetType();
                var m = t.GetMethod("GetActiveStats") ?? t.GetMethod("GetMembersStats");
                if (m != null)
                {
                    var seq = m.Invoke(party, null) as IEnumerable;
                    if (seq != null) foreach (var it in seq) if (it is CharacterStats cs) dst.Add(cs);
                }
                if (dst.Count == 0)
                {
                    var pi = t.GetProperty("Members") ?? t.GetProperty("Party");
                    var fi = t.GetField("Members") ?? t.GetField("Party");
                    var bag = (pi != null) ? pi.GetValue(party) : (fi != null ? fi.GetValue(party) : null);
                    if (bag is IEnumerable enu)
                    {
                        foreach (var it in enu)
                        {
                            var itT = it?.GetType();
                            var spi = itT?.GetProperty("Stats") ?? itT?.GetProperty("CharacterStats");
                            var stats = spi?.GetValue(it) as CharacterStats;
                            if (stats != null) dst.Add(stats);
                        }
                    }
                }
            }
            catch {}
        }

#if UNITY_2023_1_OR_NEWER
        if (dst.Count == 0) dst.AddRange(FindObjectsByType<CharacterStats>(FindObjectsInactive.Include, FindObjectsSortMode.None));
#else
        if (dst.Count == 0) dst.AddRange(FindObjectsOfType<CharacterStats>(true));
#endif
        for (int i = dst.Count - 1; i >= 0; i--) if (!dst[i] || !dst[i].gameObject.activeInHierarchy) dst.RemoveAt(i);
    }

    // ---------- API colonnes ----------
    Dictionary<PrimaryStat, int> EnsurePending(CharacterStats cs)
    {
        if (!_pending.TryGetValue(cs, out var map))
            _pending[cs] = map = new Dictionary<PrimaryStat, int>();
        return map;
    }
    Dictionary<PrimaryStat, int> GetPendingFor(CharacterStats cs) => EnsurePending(cs);

    int GetRemainingPoints(CharacterStats cs)
    {
        int unspent = GetIntByAliases(cs, ALIAS_UNSPENT, 0, out _);
        int spent   = EnsurePending(cs).Values.Sum();
        return Mathf.Max(0, unspent - spent);
    }

    void RequestAddPoint(CharacterStats who, PrimaryStat which)
    {
        if (!who) return;
        if (GetRemainingPoints(who) <= 0) return;
        var map = EnsurePending(who);
        map.TryGetValue(which, out int cur);
        map[which] = cur + 1;
        RefreshAll();
    }

    // ---------- Boutons bas ----------
    void OnCancel()
    {
        foreach (var kv in _pending) kv.Value.Clear();   // ← n’efface QUE les pending
        RefreshAll();
    }

    void OnValidate()
    {
        foreach (var kv in _pending.ToList())
        {
            var cs  = kv.Key;
            var map = kv.Value;
            if (!cs || map.Count == 0) continue;

            // Snap avant pour log
            float beforeATK = StatsPreview.Eval(cs, StatType.AttackPower, null);
            float beforeDEF = StatsPreview.Eval(cs, StatType.Defense,     null);
            float beforeHP  = StatsPreview.Eval(cs, StatType.HPMax,       null);

            // 1) Applique dans la liste Allocated (celle qu’on a vue dans tes logs)
            TryAddToAllocatedList(cs, PrimaryStat.Vitality, map.TryGetValue(PrimaryStat.Vitality, out var pVit) ? pVit : 0);
            TryAddToAllocatedList(cs, PrimaryStat.Strength, map.TryGetValue(PrimaryStat.Strength, out var pStr) ? pStr : 0);
            TryAddToAllocatedList(cs, PrimaryStat.Agility,  map.TryGetValue(PrimaryStat.Agility,  out var pAgi) ? pAgi : 0);

            int spent = pVit + pStr + pAgi;

            // 2) Décrémente Unspent si on peut écrire
            AddToIntByAliases(cs, ALIAS_UNSPENT, -spent);

            // 3) Recalcule immédiatement (plusieurs noms courants)
            CallAny(cs, RECALC);
            CallAny(GetSub(cs, "Progression"), RECALC);
            CallAny(GetSub(cs, "Stats"),       RECALC);
            CallAny(GetSub(cs, "StatSheet"),   RECALC);
            CallAny(GetSub(cs, "Formulas"),    RECALC);
            CallAny(GetSub(cs, "Calculator"),  RECALC);

            // 4) HP courant garde son ratio
            int oldHpMax = GetIntByAliases(cs, ALIAS_HP_MAX, Mathf.RoundToInt(beforeHP), out _);
            int curHp    = GetIntByAliases(cs, ALIAS_HP_CUR, oldHpMax, out _);
            int newHpMax = GetIntByAliases(cs, ALIAS_HP_MAX, oldHpMax, out _);
            float ratio  = oldHpMax > 0 ? (curHp / (float)oldHpMax) : 1f;
            SetIntByAliases(cs, ALIAS_HP_CUR, Mathf.RoundToInt(ratio * newHpMax));

            // 5) Clear pending + refresh
            map.Clear();

            float afterATK = StatsPreview.Eval(cs, StatType.AttackPower, null);
            float afterDEF = StatsPreview.Eval(cs, StatType.Defense,     null);
            float afterHP2 = StatsPreview.Eval(cs, StatType.HPMax,       null);
            Debug.Log($"[PartyStatsMenuUI] POST-VALIDATE {cs.name}  ATK {beforeATK}->{afterATK}  DEF {beforeDEF}->{afterDEF}  HPMax {beforeHP}->{afterHP2}");
        }

        RefreshAll(); // ← relit la vraie valeur (pendings vidés)
    }

    void RefreshAll()
    {
        foreach (var c in _columns) c.Refresh();
        bool hasPending = _pending.Any(kv => kv.Value.Values.Sum() > 0);
        if (validateButton) validateButton.interactable = hasPending;
        if (cancelButton)   cancelButton.interactable   = hasPending;
    }

    public void Show()   => gameObject.SetActive(true);
    public void Hide()   => gameObject.SetActive(false);
    public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);

    // ---------- Helpers ciblés (simples comme “avant”) ----------
    // On cherche UNIQUEMENT la liste Allocated “classique” vue dans tes logs
    void TryAddToAllocatedList(object root, PrimaryStat stat, int delta)
    {
        if (delta == 0 || root == null) return;

        // racine + Progression + Stats + StatSheet
        var targets = new object[] {
            GetMember(root, ALIAS_ALLOCATED),
            GetMember(GetSub(root, "Progression"), ALIAS_ALLOCATED),
            GetMember(GetSub(root, "Stats"),       ALIAS_ALLOCATED),
            GetMember(GetSub(root, "StatSheet"),   ALIAS_ALLOCATED),
        };

        foreach (var t in targets)
        {
            if (t is IList list)
            {
                var elemType = list.GetType().IsGenericType ? list.GetType().GetGenericArguments()[0] : null;
                if (elemType == null) continue;

                // Cherche enum et int “Value/Points/Amount/Count/Base”
                var enumMI = elemType.GetMembers(BF).FirstOrDefault(m =>
                    (m is PropertyInfo pi && pi.PropertyType.IsEnum) ||
                    (m is FieldInfo    fi && fi.FieldType.IsEnum));
                var intMI  = elemType.GetMembers(BF).FirstOrDefault(m =>
                    (m is PropertyInfo pi && (pi.PropertyType==typeof(int))) ||
                    (m is FieldInfo    fi && (fi.FieldType==typeof(int))));

                if (enumMI == null || intMI == null) continue;

                bool Updated(object o)
                {
                    // match enum
                    object cur = enumMI is PropertyInfo epi ? epi.GetValue(o) : ((FieldInfo)enumMI).GetValue(o);
                    bool match = cur != null && string.Equals(cur.ToString(), stat.ToString(), StringComparison.OrdinalIgnoreCase);
                    if (!match) return false;

                    // add
                    int v = (int)((intMI is PropertyInfo ip) ? ip.GetValue(o) : ((FieldInfo)intMI).GetValue(o));
                    v += delta;
                    if (intMI is PropertyInfo ip2) ip2.SetValue(o, v); else ((FieldInfo)intMI).SetValue(o, v);
                    Debug.Log($"[PartyStatsMenuUI] +{delta} → Allocated[{stat}] (list: update)");
                    return true;
                }

                bool done = false;
                for (int i = 0; i < list.Count; i++) if (Updated(list[i])) { done = true; break; }

                if (!done)
                {
                    try
                    {
                        var newElem = Activator.CreateInstance(elemType);
                        // set enum
                        if (enumMI is PropertyInfo epi3) epi3.SetValue(newElem, Enum.Parse(enumMI is PropertyInfo p ? p.PropertyType : ((FieldInfo)enumMI).FieldType, stat.ToString(), true));
                        else ((FieldInfo)enumMI).SetValue(newElem, Enum.Parse(((FieldInfo)enumMI).FieldType, stat.ToString(), true));
                        // set int
                        if (intMI is PropertyInfo ip3) ip3.SetValue(newElem, delta);
                        else ((FieldInfo)intMI).SetValue(newElem, delta);
                        list.Add(newElem);
                        Debug.Log($"[PartyStatsMenuUI] +{delta} → Allocated[{stat}] (list: add)");
                    }
                    catch { /* ignore */ }
                }
            }
        }
    }

    object GetMember(object owner, string[] aliases)
    {
        if (owner == null) return null;
        var t = owner.GetType();
        foreach (var a in aliases)
        {
            var p = t.GetProperty(a, BF);
            if (p != null) { try { return p.GetValue(owner); } catch {} }
            var f = t.GetField(a, BF);
            if (f != null) { try { return f.GetValue(owner); } catch {} }
        }
        return null;
    }
    object GetSub(object owner, string name)
    {
        if (owner == null) return null;
        var t = owner.GetType();
        var p = t.GetProperty(name, BF); if (p != null) { try { return p.GetValue(owner); } catch {} }
        var f = t.GetField(name, BF);    if (f != null) { try { return f.GetValue(owner); } catch {} }
        return null;
    }

    bool AddToIntByAliases(object obj, string[] aliases, int delta)
    {
        if (obj == null) return false;
        var t = obj.GetType();
        foreach (var a in aliases)
        {
            var p = t.GetProperty(a, BF);
            if (p != null)
            {
                try {
                    if (!p.CanWrite) continue;
                    int cur = (int)Convert.ChangeType(p.GetValue(obj), typeof(int));
                    p.SetValue(obj, cur + delta);
                    return true;
                } catch {}
            }
            var f = t.GetField(a, BF);
            if (f != null)
            {
                try {
                    int cur = (int)Convert.ChangeType(f.GetValue(obj), typeof(int));
                    f.SetValue(obj, cur + delta);
                    return true;
                } catch {}
            }
        }
        return false;
    }

    int GetIntByAliases(object obj, string[] aliases, int fallback, out string resolvedPath)
    {
        resolvedPath = null;
        if (obj == null) return fallback;
        var t = obj.GetType();
        foreach (var a in aliases)
        {
            var p = t.GetProperty(a, BF);
            if (p != null)
            {
                try { resolvedPath = a; return (int)Convert.ChangeType(p.GetValue(obj), typeof(int)); } catch {}
            }
            var f = t.GetField(a, BF);
            if (f != null)
            {
                try { resolvedPath = a; return (int)Convert.ChangeType(f.GetValue(obj), typeof(int)); } catch {}
            }
        }
        return fallback;
    }

    void SetIntByAliases(object obj, string[] aliases, int value)
    {
        if (obj == null) return;
        var t = obj.GetType();
        foreach (var a in aliases)
        {
            var p = t.GetProperty(a, BF);
            if (p != null && p.CanWrite) { try { p.SetValue(obj, value); return; } catch {} }
            var f = t.GetField(a, BF);
            if (f != null) { try { f.SetValue(obj, value); return; } catch {} }
        }
    }

    void CallAny(object obj, params string[] names)
    {
        if (obj == null) return;
        var t = obj.GetType();
        foreach (var n in names)
        {
            var mi = t.GetMethod(n, BF);
            if (mi != null) { try { mi.Invoke(obj, null); } catch {} }
        }
    }
}
