using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // pour Button, Image
using TMPro;            // <-- TMP !
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

    public Button plusHP;
    public Button plusATK;
    public Button plusDEF;

    [Header("État")]
    public bool isPlaceholder;
    public Image backdrop;

    CharacterStats _stats;
    System.Func<CharacterStats, Dictionary<PrimaryStat, int>> _getPendingFor;
    System.Func<CharacterStats, int> _getRemainingPoints;
    System.Action<CharacterStats, PrimaryStat> _requestAddPoint;

    public void SetupPlaceholder(string label = "Vide")
    {
        isPlaceholder = true;
        if (nameText) nameText.text = label;
        SetInteractable(false);
        UpdateTextsEmpty();
    }

    public void Setup(
        CharacterStats stats,
        System.Func<CharacterStats, Dictionary<PrimaryStat, int>> getPendingFor,
        System.Func<CharacterStats, int> getRemainingPoints,
        System.Action<CharacterStats, PrimaryStat> requestAddPoint)
    {
        isPlaceholder = false;
        _stats = stats;
        _getPendingFor = getPendingFor;
        _getRemainingPoints = getRemainingPoints;
        _requestAddPoint = requestAddPoint;

        SetInteractable(true);
        BindButtons();
        Refresh();
    }

    void SetInteractable(bool on)
    {
        if (backdrop) backdrop.color = on ? Color.white : new Color(1f, 1f, 1f, 0.25f);
        if (plusHP) plusHP.interactable = on;
        if (plusATK) plusATK.interactable = on;
        if (plusDEF) plusDEF.interactable = on;
    }

    void BindButtons()
    {
        if (plusHP) { plusHP.onClick.RemoveAllListeners(); plusHP.onClick.AddListener(() => _requestAddPoint?.Invoke(_stats, PrimaryStat.Vitality)); }
        if (plusDEF) { plusDEF.onClick.RemoveAllListeners(); plusDEF.onClick.AddListener(() => _requestAddPoint?.Invoke(_stats, PrimaryStat.Vitality)); }
        if (plusATK) { plusATK.onClick.RemoveAllListeners(); plusATK.onClick.AddListener(() => _requestAddPoint?.Invoke(_stats, PrimaryStat.Strength)); }
    }

    void UpdateTextsEmpty()
    {
        if (hpText) hpText.text = "-";
        if (apText) apText.text = "-";
        if (mpText) mpText.text = "-";
        if (atkText) atkText.text = "-";
        if (defText) defText.text = "-";
        if (initText) initText.text = "-";
        if (pointsLeftText) pointsLeftText.text = "Points : 0";
    }

    public void Refresh()
    {
        if (isPlaceholder || _stats == null) { UpdateTextsEmpty(); return; }

        var pending = _getPendingFor?.Invoke(_stats) ?? new Dictionary<PrimaryStat, int>();

        if (nameText) nameText.text = _stats.gameObject.name;

        float hpMax = StatsPreview.Eval(_stats, StatType.HPMax, pending);
        float apMax = StatsPreview.Eval(_stats, StatType.APMax, pending);
        float mpMax = StatsPreview.Eval(_stats, StatType.MPMax, pending);
        float atk = StatsPreview.Eval(_stats, StatType.AttackPower, pending);
        float def = StatsPreview.Eval(_stats, StatType.Defense, pending);
        float init = StatsPreview.Eval(_stats, StatType.Initiative, pending);

        if (hpText) hpText.text = $"HP : {Mathf.RoundToInt(hpMax)}";
        if (apText) apText.text = $"AP : {Mathf.RoundToInt(apMax)}";
        if (mpText) mpText.text = $"MP : {Mathf.RoundToInt(mpMax)}";
        if (atkText) atkText.text = $"ATK : {Mathf.RoundToInt(atk)}";
        if (defText) defText.text = $"DEF : {Mathf.RoundToInt(def)}";
        if (initText) initText.text = $"INIT : {Mathf.RoundToInt(init)}";

        int remaining = _getRemainingPoints?.Invoke(_stats) ?? 0;
        if (pointsLeftText) pointsLeftText.text = $"Points : {remaining}";

        bool canAdd = remaining > 0;
        if (plusHP) plusHP.interactable = canAdd;
        if (plusATK) plusATK.interactable = canAdd;
        if (plusDEF) plusDEF.interactable = canAdd;
        Debug.Log($"[SheetColumn] refresh {(isPlaceholder ? "placeholder" : _stats?.name)}");

    }
    
}
