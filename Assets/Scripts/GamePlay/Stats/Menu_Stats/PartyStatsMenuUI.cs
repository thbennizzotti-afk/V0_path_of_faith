using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathOfFaith.Gameplay.Stats;

[DisallowMultipleComponent]
public class PartyStatsMenuUI : MonoBehaviour
{
    [Header("Entrées / Références")]
    public PartyService party;                  // _Service → PartyService
    public RectTransform columnsContainer;      // parent avec GridLayoutGroup (4 colonnes)
    public CharacterSheetColumnUI columnPrefab; // prefab d’une colonne (TMP)
    public Button validateButton;
    public Button closeButton;

    [Header("Option : bloquer les contrôles d'exploration quand le menu est ouvert")]
    public GameObject explorationSystemsRoot;   // parent de tes scripts d'explo (facultatif)

    // État runtime
    readonly Dictionary<CharacterStats, Dictionary<PrimaryStat,int>> _pending = new();
    readonly List<CharacterSheetColumnUI> _columns = new();
    bool _wired; // pour ne câbler les listeners qu'une fois

    void Awake()
    {
        if (!party)
        {
#if UNITY_6000_0_OR_NEWER
            party = FindFirstObjectByType<PartyService>(FindObjectsInactive.Include);
#else
            party = FindObjectOfType<PartyService>();
#endif
        }
    }

    void OnEnable()
    {
        WireButtonsOnce();
        if (explorationSystemsRoot) explorationSystemsRoot.SetActive(false);
        BuildColumns();
        RefreshAll();
    }

    void OnDisable()
    {
        if (explorationSystemsRoot) explorationSystemsRoot.SetActive(true);
    }

    void WireButtonsOnce()
    {
        if (_wired) return;
        if (validateButton) validateButton.onClick.AddListener(ApplyAll);
        if (closeButton)    closeButton.onClick.AddListener(Hide);
        _wired = true;
    }

    // Appelle ça si tu veux ouvrir/fermer manuellement depuis un autre script
    public void Show()  { gameObject.SetActive(true);  }
    public void Hide()  { gameObject.SetActive(false); }
    public void Toggle(){ gameObject.SetActive(!gameObject.activeSelf); }

    // -------- Construction des colonnes --------
    void BuildColumns()
    {
        if (!columnsContainer || !columnPrefab) { Debug.LogWarning("[PartyStatsMenuUI] Références manquantes."); return; }

        // Clear
        for (int i = columnsContainer.childCount - 1; i >= 0; i--)
        {
            var c = columnsContainer.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(c.gameObject);
            else                        Destroy(c.gameObject);
#else
            Destroy(c.gameObject);
#endif
        }
        _columns.Clear();

        // 4 slots (0..3)
        for (int i = 0; i < 4; i++)
        {
            var col = Instantiate(columnPrefab, columnsContainer);
            _columns.Add(col);

            var member = party ? party.GetBySlot(i) : null;
            if (member == null || member.stats == null)
            {
                col.SetupPlaceholder($"(vide {i+1})");
                continue;
            }

            if (!_pending.ContainsKey(member.stats))
                _pending[member.stats] = new Dictionary<PrimaryStat,int>();

            col.Setup(
                member.stats,
                getPendingFor: GetPendingFor,
                getRemainingPoints: GetRemainingFor,
                requestAddPoint: RequestAddPoint);
        }
    }

    Dictionary<PrimaryStat,int> GetPendingFor(CharacterStats s)
    {
        if (!_pending.TryGetValue(s, out var d))
        {
            d = new Dictionary<PrimaryStat,int>();
            _pending[s] = d;
        }
        return d;
    }

    int GetRemainingFor(CharacterStats s)
    {
        int used = 0;
        if (_pending.TryGetValue(s, out var d))
            foreach (var kv in d) used += kv.Value;
        return Mathf.Max(0, s.unspentAttributePoints - used);
    }

    void RequestAddPoint(CharacterStats s, PrimaryStat stat)
    {
        if (s == null) return;
        if (GetRemainingFor(s) <= 0) return;

        var bag = GetPendingFor(s);
        bag.TryGetValue(stat, out int cur);
        bag[stat] = cur + 1;

        RefreshAll();
    }

    void ApplyAll()
    {
        // Applique les allocations en attente, perso par perso
        var keys = new List<CharacterStats>(_pending.Keys);
        foreach (var stats in keys)
        {
            if (!_pending.TryGetValue(stats, out var bag)) continue;

            foreach (var alloc in bag)
            {
                // Applique point par point pour respecter les contrôles internes
                for (int i = 0; i < alloc.Value; i++)
                    stats.AllocatePoints(alloc.Key, 1);
            }
            // Clear les pending pour ce perso
            _pending[stats].Clear();
        }

        RefreshAll();
    }

    void RefreshAll()
    {
        foreach (var col in _columns) col.Refresh();

        // Activer/désactiver le bouton "Valider" selon s'il reste des points en attente
        bool anyPending = false;
        foreach (var kv in _pending)
        {
            foreach (var p in kv.Value)
            {
                if (p.Value > 0) { anyPending = true; break; }
            }
            if (anyPending) break;
        }
        if (validateButton) validateButton.interactable = anyPending;
    }
}
