// Assets/Scripts/Fondation/UI/PauseSaveSlotsUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathOfFaith.Fondation.Core; // ServiceLocator
using PathOfFaith.Save;           // SaveManager

public class PauseSaveSlotsUI : MonoBehaviour
{
    [Header("Boutons slot 1..3 (dans l'ordre)")]
    public Button[] slotButtons;

    [Header("Labels TMP_Text 1..3 (dans l'ordre)")]
    public TMP_Text[] slotLabels;

    SaveManager _mgr;

    void Awake()
    {
        _mgr = ServiceLocator.Get<SaveManager>();

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;
            if (slotButtons[i])
                slotButtons[i].onClick.AddListener(() => OnClickSlot(idx));
        }
    }

    void OnEnable() => Refresh();

    public void Refresh()
    {
        if (_mgr == null) return;

        for (int i = 0; i < 3; i++)
        {
            var slot = SlotIds.All[i];
            var info = _mgr.GetInfo(slot); // (ok, savedAt, scene, version)

            if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
            {
                slotLabels[i].text = info.ok
                    ? $"Slot {i+1} • {info.scene} • {info.savedAt.ToLocalTime():yyyy-MM-dd HH:mm}"
                    : $"Slot {i+1} • (vide)";
            }
        }
    }

    void OnClickSlot(int idx)
    {
        var slot = SlotIds.All[idx];
        _mgr.Save(slot);    // écrase si déjà présent
        Refresh();
        gameObject.SetActive(false); // referme le picker
    }
}
