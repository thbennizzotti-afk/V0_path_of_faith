// Assets/Scripts/Fondation/Save/MainMenuLoadSlotsUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PathOfFaith.Fondation.Core;
using PathOfFaith.Save;
using PathOfFaith.App;

public class MainMenuLoadSlotsUI : MonoBehaviour
{
    [Header("Boutons slot 1..3 (dans l'ordre)")]
    public Button[] slotButtons;

    [Header("Labels TMP_Text 1..3 (dans l'ordre)")]
    public TMP_Text[] slotLabels;

    ISaveService _save;   // non utilisé ici, mais dispo si besoin
    SaveManager  _mgr;

    void Awake()
    {
        _save = ServiceLocator.Get<ISaveService>();
        _mgr  = ServiceLocator.Get<SaveManager>();

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;
            if (slotButtons[i])
                slotButtons[i].onClick.AddListener(() => OnClickLoad(idx));
        }
    }

    void OnEnable() => Refresh();

    void Refresh()
    {
        for (int i = 0; i < 3; i++)
        {
            var slot = SlotIds.All[i];
            var info = _mgr.GetInfo(slot);

            if (slotLabels != null && i < slotLabels.Length && slotLabels[i])
            {
                slotLabels[i].text = info.ok
                    ? $"Slot {i+1} • {info.scene} • {info.savedAt.ToLocalTime():yyyy-MM-dd HH:mm}"
                    : $"Slot {i+1} • (vide)";
            }

            if (slotButtons != null && i < slotButtons.Length && slotButtons[i])
                slotButtons[i].interactable = info.ok;
        }
    }

    void OnClickLoad(int idx)
    {
        var slot = SlotIds.All[idx];
        var info = _mgr.GetInfo(slot);
        if (!info.ok) return;

        // On “demande” le chargement, puis on bascule sur la scène de la save.
        StartOptions.RequestLoad(slot);
        SceneManager.LoadScene(info.scene);
    }

    public void OnClickBack()
    {
        gameObject.SetActive(false);
    }
}
