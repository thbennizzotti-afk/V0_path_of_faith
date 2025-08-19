using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathOfFaith.Gameplay.Stats;

[DisallowMultipleComponent]
public class CharacterSheetColumnUI : MonoBehaviour
{
    [Header("Références UI (TMP)")]
    public TMP_Text nameText, hpText, apText, mpText, atkText, defText, initText, pointsLeftText;

    [Header("Boutons +")]
    public Button plusHP, plusATK, plusDEF;

    [Header("Style")]
    public Vector2 plusButtonSize = new Vector2(36f, 36f);
    public float rowMinHeight = 36f;
    [Range(50,120)] public int plusFontPercent = 85;

    [Header("État")]
    public bool isPlaceholder;
    public Image backdrop;

    CharacterStats _stats;
    Func<CharacterStats, Dictionary<PrimaryStat,int>> _getPendingFor;
    Func<CharacterStats, int> _getRemainingPoints;
    Action<CharacterStats, PrimaryStat> _requestAddPoint;

    void Awake()
    {
        NormalizeTMP(nameText); NormalizeTMP(hpText); NormalizeTMP(apText); NormalizeTMP(mpText);
        NormalizeTMP(atkText);  NormalizeTMP(defText); NormalizeTMP(initText); NormalizeTMP(pointsLeftText);

        if (!plusHP || !plusATK || !plusDEF)
        {
            Debug.LogError("[SheetColumn] Assigne PlusHP/PlusATK/PlusDEF dans l’Inspector.");
            enabled = false; return;
        }

        EnsureButtonUsable(plusHP,  plusButtonSize, plusFontPercent);
        EnsureButtonUsable(plusATK, plusButtonSize, plusFontPercent);
        EnsureButtonUsable(plusDEF, plusButtonSize, plusFontPercent);
    }

    void OnEnable()
    {
        if (!isPlaceholder && _stats != null)
        {
            BindButtons();
            StartCoroutine(SanitizeOverFrames());
        }
    }

    void OnDisable() => UnbindButtons();

    public void SetupPlaceholder(string label = "Vide")
    {
        isPlaceholder = true;
        _stats = null; _getPendingFor = null; _getRemainingPoints = null; _requestAddPoint = null;
        UnbindButtons();
        if (nameText) nameText.text = label;
        SetInteractable(false);
        UpdateTextsEmpty();
    }

    public void Setup(
        CharacterStats stats,
        Func<CharacterStats, Dictionary<PrimaryStat,int>> getPendingFor,
        Func<CharacterStats, int> getRemainingPoints,
        Action<CharacterStats, PrimaryStat> requestAddPoint)
    {
        isPlaceholder = false;
        _stats = stats;
        _getPendingFor = getPendingFor;
        _getRemainingPoints = getRemainingPoints;
        _requestAddPoint = requestAddPoint;

        SetInteractable(true);
        BindButtons();
        Refresh();

        SanitizeAllRows();
        StartCoroutine(SanitizeOverFrames());
    }

    public void Refresh()
    {
        if (isPlaceholder || !_stats) { UpdateTextsEmpty(); return; }

        var pending = _getPendingFor?.Invoke(_stats) ?? new Dictionary<PrimaryStat,int>();
        int pStr = pending.TryGetValue(PrimaryStat.Strength, out var s) ? s : 0;
        int pAgi = pending.TryGetValue(PrimaryStat.Agility,  out var a) ? a : 0;

        if (nameText) nameText.text = _stats.gameObject.name;

        float hpMax = StatsPreview.Eval(_stats, StatType.HPMax, pending);
        float apMax = StatsPreview.Eval(_stats, StatType.APMax, pending);
        float mpMax = StatsPreview.Eval(_stats, StatType.MPMax, pending);
        float atk   = StatsPreview.Eval(_stats, StatType.AttackPower, pending);
        float def   = StatsPreview.Eval(_stats, StatType.Defense, pending);
        float init  = StatsPreview.Eval(_stats, StatType.Initiative, pending);

        if (hpText)   hpText.text = $"HP : {Mathf.RoundToInt(hpMax)}";
        if (apText)   apText.text = $"AP : {Mathf.RoundToInt(apMax)}";
        if (mpText)   mpText.text = $"MP : {Mathf.RoundToInt(mpMax)}";
        if (atkText)  atkText.text = $"ATK : {WithDelta(Mathf.RoundToInt(atk), pStr)}";
        if (defText)  defText.text = $"DEF : {WithDelta(Mathf.RoundToInt(def), pAgi)}";
        if (initText) initText.text = $"INIT : {Mathf.RoundToInt(init)}";

        int remaining = _getRemainingPoints?.Invoke(_stats) ?? 0;
        if (pointsLeftText) pointsLeftText.text = $"Points : {remaining}";

        bool canAdd = remaining > 0;
        if (plusHP)  plusHP.interactable = canAdd;
        if (plusATK) plusATK.interactable = canAdd;
        if (plusDEF) plusDEF.interactable = canAdd;

        Debug.Log($"[SheetColumn] refresh {_stats.name} (restants={remaining})");
    }

    void BindButtons()
    {
        UnbindButtons();

        if (_requestAddPoint == null) { Debug.LogWarning("[SheetColumn] pas de callback _requestAddPoint."); return; }

        plusHP.onClick.AddListener(() => { Debug.Log("[UI] +HP cliqué");  _requestAddPoint(_stats, PrimaryStat.Vitality); });
        plusATK.onClick.AddListener(() => { Debug.Log("[UI] +ATK cliqué"); _requestAddPoint(_stats, PrimaryStat.Strength); });
        plusDEF.onClick.AddListener(() => { Debug.Log("[UI] +DEF cliqué (Agi)"); _requestAddPoint(_stats, PrimaryStat.Agility); });
    }

    void UnbindButtons()
    {
        if (plusHP)  plusHP.onClick.RemoveAllListeners();
        if (plusATK) plusATK.onClick.RemoveAllListeners();
        if (plusDEF) plusDEF.onClick.RemoveAllListeners();
    }

    void SetInteractable(bool on)
    {
        if (backdrop) backdrop.color = on ? Color.white : new Color(1,1,1,0.25f);
        if (plusHP) plusHP.interactable = on;
        if (plusATK) plusATK.interactable = on;
        if (plusDEF) plusDEF.interactable = on;
    }

    void UpdateTextsEmpty()
    {
        if (hpText) hpText.text = "-"; if (apText) apText.text = "-"; if (mpText) mpText.text = "-";
        if (atkText) atkText.text = "-"; if (defText) defText.text = "-"; if (initText) initText.text = "-";
        if (pointsLeftText) pointsLeftText.text = "Points : 0";
    }

    static string WithDelta(int baseVal, int delta) => delta > 0 ? $"{baseVal} (+{delta})" : baseVal.ToString();

    static void NormalizeTMP(TMP_Text t)
    {
        if (!t) return;
        if (t.font) t.fontMaterial = t.font.material;
        t.color = new Color32(34,34,34,255);
        t.raycastTarget = false;
        if (t.rectTransform.localScale == Vector3.zero) t.rectTransform.localScale = Vector3.one;
    }

    // ------- taille/label bouton + helpers -------
    static void EnsureButtonUsable(Button b, Vector2 size, int fontPercent)
    {
        if (!b) return;

        if (b.targetGraphic == null)
        {
            var img = b.GetComponent<Image>();
            if (img) { b.targetGraphic = img; img.raycastTarget = true; }
        }
        else b.targetGraphic.raycastTarget = true;

        var le = b.GetComponent<LayoutElement>() ?? b.gameObject.AddComponent<LayoutElement>();
        le.minWidth = le.preferredWidth = size.x;
        le.minHeight = le.preferredHeight = size.y;
        le.flexibleWidth = 0; le.flexibleHeight = 0;

        var rt = b.GetComponent<RectTransform>();
        if (rt)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        var tmp = b.GetComponentInChildren<TMP_Text>(true);
        if (!tmp)
        {
            var go = new GameObject("PlusLabel", typeof(RectTransform));
            go.transform.SetParent(b.transform, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;

            tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "+";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color32(34,34,34,255);
        }
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = Mathf.RoundToInt(Mathf.Min(size.x, size.y) * (fontPercent / 100f));

        b.navigation = new Navigation { mode = Navigation.Mode.None };
    }

    static void EnsureRowLayout(Transform row, float minH)
    {
        if (!row) return;
        var le = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = minH;
        le.flexibleHeight = 0;
    }

    IEnumerator SanitizeOverFrames()
    {
        SanitizeAllRows(); yield return null;
        SanitizeAllRows(); yield return null;
        SanitizeAllRows();
    }

    void SanitizeAllRows()
    {
        SanitizeRow("Row_HP",  plusHP);
        SanitizeRow("Row_ATK", plusATK);
        SanitizeRow("Row_DEF", plusDEF);
    }

    void SanitizeRow(string rowName, Button keep)
    {
        var row = transform.Find(rowName) ?? GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == rowName);
        if (!row || !keep) return;

        EnsureRowLayout(row, rowMinHeight);

        bool keepHasLabel = keep.GetComponentInChildren<TMP_Text>(true) != null;

        var buttons = row.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (!b || b == keep) continue;

            if (!keepHasLabel)
            {
                var lbl = b.GetComponentInChildren<TMP_Text>(true);
                if (lbl)
                {
                    lbl.raycastTarget = false;
                    lbl.transform.SetParent(keep.transform, false);
                    var r = lbl.rectTransform;
                    r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                    r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
                    keepHasLabel = true;
                }
            }

            if (b.targetGraphic) b.targetGraphic.raycastTarget = false;
            b.interactable = false;
            Debug.Log($"[SheetColumn] Doublon bouton supprimé dans {rowName}: {b.name}");
            Destroy(b.gameObject);
        }

        keep.transform.SetAsLastSibling();
        EnsureButtonUsable(keep, plusButtonSize, plusFontPercent);
    }

#if UNITY_EDITOR
    [ContextMenu("Apply Style Now")]
    public void ApplyStyleNow()
    {
        EnsureButtonUsable(plusHP,  plusButtonSize, plusFontPercent);
        EnsureButtonUsable(plusATK, plusButtonSize, plusFontPercent);
        EnsureButtonUsable(plusDEF, plusButtonSize, plusFontPercent);
        SanitizeAllRows();
    }
    void OnValidate() { if (Application.isPlaying) ApplyStyleNow(); }
#endif
}
