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
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text apText;
    public TMP_Text mpText;
    public TMP_Text atkText;
    public TMP_Text defText;
    public TMP_Text initText;
    public TMP_Text pointsLeftText;

    [Header("Boutons +")]
    public Button plusHP;
    public Button plusATK;
    public Button plusDEF;

    [Header("Style")]
    public Vector2 plusButtonSize = new Vector2(36f, 36f); // taille souhaitée des boutons +
    public float rowMinHeight = 36f;                       // hauteur mini de chaque Row_*
    [Range(50, 120)] public int plusFontPercent = 80;      // taille du " + " = % du côté le plus petit

    [Header("État")]
    public bool isPlaceholder;
    public Image backdrop;

    // Callbacks fournis par PartyStatsMenuUI
    CharacterStats _stats;
    Func<CharacterStats, Dictionary<PrimaryStat, int>> _getPendingFor;
    Func<CharacterStats, int> _getRemainingPoints;
    Action<CharacterStats, PrimaryStat> _requestAddPoint;

    // ---------- Lifecycle ----------
    void Awake()
    {
        // Normalise les TMP et coupe leurs raycasts (pour ne pas manger les clics)
        NormalizeTMP(nameText);
        NormalizeTMP(hpText);
        NormalizeTMP(apText);
        NormalizeTMP(mpText);
        NormalizeTMP(atkText);
        NormalizeTMP(defText);
        NormalizeTMP(initText);
        NormalizeTMP(pointsLeftText);

        // Exige les trois boutons : on NE crée rien à la volée.
        if (!plusHP || !plusATK || !plusDEF)
        {
            Debug.LogError("[SheetColumn] Boutons + non assignés dans l’Inspector (PlusHP / PlusATK / PlusDEF). Assigne-les sur le prefab.");
            enabled = false;
            return;
        }

        // Prépare boutons (raycasts, label "+", taille) tout de suite
        EnsureButtonUsable(plusHP,  plusButtonSize, plusFontPercent);
        EnsureButtonUsable(plusATK, plusButtonSize, plusFontPercent);
        EnsureButtonUsable(plusDEF, plusButtonSize, plusFontPercent);
    }

    void OnEnable()
    {
        if (!isPlaceholder && _stats != null)
        {
            BindButtons();
            // Nettoie doublons + réimpose tailles/hauteurs sur plusieurs frames
            StartCoroutine(SanitizeOverFrames());
        }
    }

    void OnDisable()
    {
        UnbindButtons();
    }

    // ---------- Public API ----------
    public void SetupPlaceholder(string label = "Vide")
    {
        isPlaceholder = true;
        _stats = null;
        _getPendingFor = null;
        _getRemainingPoints = null;
        _requestAddPoint = null;

        UnbindButtons();
        if (nameText) nameText.text = label;
        SetInteractable(false);
        UpdateTextsEmpty();
    }

    public void Setup(
        CharacterStats stats,
        Func<CharacterStats, Dictionary<PrimaryStat, int>> getPendingFor,
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

        // Nettoie / impose tailles maintenant + frames suivantes
        SanitizeAllRows();
        StartCoroutine(SanitizeOverFrames());
    }

    public void Refresh()
    {
        if (isPlaceholder || _stats == null)
        {
            UpdateTextsEmpty();
            return;
        }

        var pending = _getPendingFor?.Invoke(_stats) ?? new Dictionary<PrimaryStat, int>();
        int pVit = pending.TryGetValue(PrimaryStat.Vitality, out var v) ? v : 0;
        int pStr = pending.TryGetValue(PrimaryStat.Strength, out var s) ? s : 0;
        int pAgi = pending.TryGetValue(PrimaryStat.Agility, out var a) ? a : 0; // DEF dérive d'Agility

        if (nameText) nameText.text = _stats.gameObject.name;

        float hpMax = StatsPreview.Eval(_stats, StatType.HPMax, pending);
        float apMax = StatsPreview.Eval(_stats, StatType.APMax, pending);
        float mpMax = StatsPreview.Eval(_stats, StatType.MPMax, pending);
        float atk   = StatsPreview.Eval(_stats, StatType.AttackPower, pending);
        float def   = StatsPreview.Eval(_stats, StatType.Defense, pending);
        float init  = StatsPreview.Eval(_stats, StatType.Initiative, pending);

        if (hpText)   hpText.text   = $"HP : {Mathf.RoundToInt(hpMax)}";
        if (apText)   apText.text   = $"AP : {Mathf.RoundToInt(apMax)}";
        if (mpText)   mpText.text   = $"MP : {Mathf.RoundToInt(mpMax)}";
        if (atkText)  atkText.text  = $"ATK : {WithDelta(Mathf.RoundToInt(atk), pStr)}";
        if (defText)  defText.text  = $"DEF : {WithDelta(Mathf.RoundToInt(def), pAgi)}";
        if (initText) initText.text = $"INIT : {Mathf.RoundToInt(init)}";

        int remaining = _getRemainingPoints?.Invoke(_stats) ?? 0;
        if (pointsLeftText) pointsLeftText.text = $"Points : {remaining}";

        bool canAdd = remaining > 0;
        if (plusHP)  plusHP.interactable  = canAdd;
        if (plusATK) plusATK.interactable = canAdd;
        if (plusDEF) plusDEF.interactable = canAdd;

        Debug.Log($"[SheetColumn] refresh {(_stats ? _stats.name : "placeholder")} (restants={remaining})");
    }

    // ---------- Internes ----------
    void BindButtons()
    {
        UnbindButtons();

        if (_requestAddPoint == null)
        {
            Debug.LogWarning("[SheetColumn] _requestAddPoint non fourni. Les + ne feront rien.");
            return;
        }

        if (plusHP)
            plusHP.onClick.AddListener(() =>
            {
                Debug.Log("[UI] +HP cliqué");
                _requestAddPoint.Invoke(_stats, PrimaryStat.Vitality);
            });

        if (plusATK)
            plusATK.onClick.AddListener(() =>
            {
                Debug.Log("[UI] +ATK cliqué");
                _requestAddPoint.Invoke(_stats, PrimaryStat.Strength);
            });

        if (plusDEF)
            plusDEF.onClick.AddListener(() =>
            {
                Debug.Log("[UI] +DEF cliqué (redirigé vers Agility)");
                _requestAddPoint.Invoke(_stats, PrimaryStat.Agility);
            });
    }

    void UnbindButtons()
    {
        if (plusHP)  plusHP.onClick.RemoveAllListeners();
        if (plusATK) plusATK.onClick.RemoveAllListeners();
        if (plusDEF) plusDEF.onClick.RemoveAllListeners();
    }

    void SetInteractable(bool on)
    {
        if (backdrop) backdrop.color = on ? Color.white : new Color(1f, 1f, 1f, 0.25f);
        if (plusHP)  plusHP.interactable  = on;
        if (plusATK) plusATK.interactable = on;
        if (plusDEF) plusDEF.interactable = on;
    }

    void UpdateTextsEmpty()
    {
        if (hpText)         hpText.text = "-";
        if (apText)         apText.text = "-";
        if (mpText)         mpText.text = "-";
        if (atkText)        atkText.text = "-";
        if (defText)        defText.text = "-";
        if (initText)       initText.text = "-";
        if (pointsLeftText) pointsLeftText.text = "Points : 0";
    }

    static string WithDelta(int baseVal, int delta)
        => delta > 0 ? $"{baseVal} (+{delta})" : baseVal.ToString();

    static void NormalizeTMP(TMP_Text t)
    {
        if (!t) return;
        if (t.font) t.fontMaterial = t.font.material;
        t.color = new Color32(34, 34, 34, 255);
        t.raycastTarget = false;
        if (t.rectTransform.localScale == Vector3.zero)
            t.rectTransform.localScale = Vector3.one;
    }

    // impose raycast, label "+", taille via LayoutElement
    static void EnsureButtonUsable(Button b, Vector2 size, int fontPercent)
    {
        if (!b) return;

        // Target Graphic
        if (b.targetGraphic == null)
        {
            var img = b.GetComponent<Image>();
            if (img) { b.targetGraphic = img; img.raycastTarget = true; }
        }
        else b.targetGraphic.raycastTarget = true;

        // Taille via LayoutElement
        var le = b.GetComponent<LayoutElement>();
        if (!le) le = b.gameObject.AddComponent<LayoutElement>();
        le.minWidth = le.preferredWidth = size.x;
        le.minHeight = le.preferredHeight = size.y;
        le.flexibleWidth = 0; le.flexibleHeight = 0;

        // Label "+"
        var tmp = b.GetComponentInChildren<TMP_Text>(true);
        if (!tmp)
        {
            var go = new GameObject("PlusLabel", typeof(RectTransform));
            go.transform.SetParent(b.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "+";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color32(34, 34, 34, 255);
        }
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = Mathf.RoundToInt(Mathf.Min(size.x, size.y) * (fontPercent / 100f));

        // Navigation None
        b.navigation = new Navigation { mode = Navigation.Mode.None };
    }

    static void EnsureRowLayout(Transform row, float minHeight)
    {
        if (!row) return;
        var le = row.GetComponent<LayoutElement>();
        if (!le) le = row.gameObject.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = minHeight;
        le.flexibleHeight = 0;
    }

    // ---------- Anti-doublons + tailles/hauteurs imposées ----------
    IEnumerator SanitizeOverFrames()
    {
        SanitizeAllRows();
        yield return null;
        SanitizeAllRows();
        yield return null;
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
        var row = transform.Find(rowName);
        if (!row)
        {
            row = GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == rowName);
            if (!row) return;
        }
        if (!keep) return;

        // Empêche l’écrasement de la hauteur
        EnsureRowLayout(row, rowMinHeight);

        // Si le label "+" manque sur le bon bouton, on tente de le récupérer d’un doublon
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
                    var rt = lbl.rectTransform;
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                    keepHasLabel = true;
                }
            }

            var g = b.targetGraphic;
            if (g) g.raycastTarget = false;
            b.interactable = false;

            Debug.Log($"[SheetColumn] Doublon bouton supprimé dans {rowName}: {b.name}");
            Destroy(b.gameObject);
        }

        // Dessin au-dessus + impose la taille et la police
        keep.transform.SetAsLastSibling();
        EnsureButtonUsable(keep, plusButtonSize, plusFontPercent);
    }
}
