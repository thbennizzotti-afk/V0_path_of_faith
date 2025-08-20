using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Save;

[DisallowMultipleComponent]
public class PauseSaveSlotsUI : MonoBehaviour
{
    [Header("Boutons slot 1..3 (dans l'ordre)")]
    public Button[] slotButtons;          // 3 boutons

    [Header("Labels TMP_Text 1..3 (dans l'ordre)")]
    public TMP_Text[] slotLabels;         // 3 labels (TMP)

    ISaveService _save;
    SaveManager  _mgr;

    void Awake()
    {
        _save = ServiceLocator.Get<ISaveService>();
        _mgr  = ServiceLocator.Get<SaveManager>();

        // câblage des clics
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i; // capture
            if (slotButtons[i] != null)
                slotButtons[i].onClick.AddListener(() => OnClickSlot(idx));
        }
    }

    void OnEnable() => Refresh();

    // Doit être public (appelé depuis PauseMenuActions)
    public void Refresh()
    {
        for (int i = 0; i < 3; i++)
        {
            var slot = SlotIds.All[i];
            var info = _mgr.GetInfo(slot);

            if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
            {
                slotLabels[i].text = info.ok
                    ? $"Slot {i + 1} • {info.scene} • {info.savedAt.ToLocalTime():yyyy-MM-dd HH:mm}"
                    : $"Slot {i + 1} • (vide)";
            }
        }
    }

    void OnClickSlot(int idx)
    {
        var slot = SlotIds.All[idx];
        _save.Save(slot);          // écrase si déjà présent
        Refresh();                 // met à jour les libellés
        gameObject.SetActive(false); // referme le panneau de slots
    }
}
