// Assets/Scripts/Fondation/UI/MainMenuLoadSlotsUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PathOfFaith.App;            // StartOptions
using PathOfFaith.Fondation.Core; // ServiceLocator
using PathOfFaith.Save;           // SaveManager

public class MainMenuLoadSlotsUI : MonoBehaviour
{
    [SerializeField] Button[]  slotButtons;
    [SerializeField] TMP_Text[] slotLabels;
    [SerializeField] string gameSceneName = "Game";

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
            var info = _mgr.GetInfo(slot);
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
        var info = _mgr.GetInfo(slot);

        if (info.ok) StartOptions.RequestLoad(slot);
        else         StartOptions.NewGame();

        SceneManager.LoadScene(gameSceneName);
    }
}
