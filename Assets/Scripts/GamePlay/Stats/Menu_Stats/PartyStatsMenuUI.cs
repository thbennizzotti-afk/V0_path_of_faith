using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PathOfFaith.Gameplay.Stats;

public class PartyStatsMenuUI : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Met ton PartyService (sous _Service) ici (facultatif). Sinon on scanne la scène.")]
    public PartyService party;
    [Tooltip("Le conteneur qui a le Grid Layout Group")]
    public Transform columnsContainer;
    [Tooltip("Prefab de colonne (version TMP)")]
    public CharacterSheetColumnUI columnPrefab;

    [Header("Bas de fenêtre")]
    public Button validateButton;
    public Button cancelButton;
    public Button closeButton;

    [Header("Options")]
    [Range(1, 8)] public int maxColumns = 4;
    [Tooltip("Conserver le ratio de PV courant quand le max change à la validation")]
    public bool keepHpRatioOnValidate = true;

    // ---- interne ----
    readonly List<CharacterStats> _members = new();
    readonly List<CharacterSheetColumnUI> _columns = new();
    // pending par perso : combien de points ajoutés par carac (avant validation)
    readonly Dictionary<CharacterStats, Dictionary<PrimaryStat, int>> _pending = new();

    // ---------- Cycle ----------
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

    // ---------- Construction UI ----------
    void BuildColumns()
    {
        if (!columnsContainer || !columnPrefab)
        {
            Debug.LogError("[PartyStatsMenuUI] columnsContainer/columnPrefab non assignés.");
            return;
        }

        // wipe
        foreach (Transform c in columnsContainer) Destroy(c.gameObject);
        _columns.Clear();
        _members.Clear();

        // récup membres
        CollectMembers(_members);

        // colonnes
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
        if (party)
        {
            foreach (var m in party.Members)
                if (m && m.stats && m.gameObject.activeInHierarchy)
                    dst.Add(m.stats);
        }
        else
        {
            // fallback : scène
#if UNITY_2023_1_OR_NEWER
            var pms = FindObjectsByType<PartyMember>(FindObjectsSortMode.None);
#else
            var pms = FindObjectsOfType<PartyMember>(true);
#endif
            foreach (var pm in pms)
                if (pm && pm.stats && pm.gameObject.activeInHierarchy)
                    dst.Add(pm.stats);
        }
    }

    // ---------- API fournie aux colonnes ----------
    Dictionary<PrimaryStat, int> EnsurePending(CharacterStats cs)
    {
        if (!_pending.TryGetValue(cs, out var map))
            _pending[cs] = map = new Dictionary<PrimaryStat, int>();
        return map;
    }
    Dictionary<PrimaryStat, int> GetPendingFor(CharacterStats cs) => EnsurePending(cs);

    int GetRemainingPoints(CharacterStats cs)
    {
        if (!cs) return 0;
        int spentPreview = EnsurePending(cs).Values.Sum();
        int remain = Mathf.Max(0, cs.unspentAttributePoints - spentPreview);
        return remain;
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

    // ---------- Bas de fenêtre ----------
    void OnCancel()
    {
        foreach (var kv in _pending) kv.Value.Clear(); // on efface juste la preview
        RefreshAll();
    }

    void OnValidate()
    {
        foreach (var kv in _pending.ToList())
        {
            var cs = kv.Key;
            var add = kv.Value; // {stat -> points ajoutés}
            if (!cs || add.Count == 0) continue;

            // 1) snapshot pour ratio HP
            int oldMax = Mathf.RoundToInt(cs.GetStat(StatType.HPMax));
            int curHP  = cs.hpCurrent;

            // 2) on applique via l’API officielle -> gère unspent + recalc
            if (add.TryGetValue(PrimaryStat.Vitality, out int pVit) && pVit > 0)
                cs.AllocatePoints(PrimaryStat.Vitality, pVit);
            if (add.TryGetValue(PrimaryStat.Strength, out int pStr) && pStr > 0)
                cs.AllocatePoints(PrimaryStat.Strength, pStr);
            if (add.TryGetValue(PrimaryStat.Agility, out int pAgi) && pAgi > 0)
                cs.AllocatePoints(PrimaryStat.Agility, pAgi);
            if (add.TryGetValue(PrimaryStat.Intellect, out int pInt) && pInt > 0)
                cs.AllocatePoints(PrimaryStat.Intellect, pInt);

            // 3) ajustement PV courant (après RebuildSheet fait par AllocatePoints)
            if (keepHpRatioOnValidate)
            {
                int newMax = Mathf.RoundToInt(cs.GetStat(StatType.HPMax));
                if (oldMax > 0 && newMax > 0)
                {
                    float ratio = curHP / (float)oldMax;
                    cs.hpCurrent = Mathf.RoundToInt(newMax * ratio);
                }
            }

            // 4) fin
            add.Clear();
        }

        RefreshAll(); // relit tout : points restants, preview, etc.
    }

    void RefreshAll()
    {
        foreach (var c in _columns) c.Refresh();

        bool hasPending = _pending.Any(p => p.Value.Values.Sum() > 0);
        if (validateButton) validateButton.interactable = hasPending;
        if (cancelButton)   cancelButton.interactable   = hasPending;
    }

    // ---------- Ouverture / fermeture ----------
    public void Show()   => gameObject.SetActive(true);
    public void Hide()   => gameObject.SetActive(false);
    public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);
}
