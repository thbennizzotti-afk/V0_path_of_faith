// StatsMenuHotkey.cs
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class StatsMenuHotkey : MonoBehaviour
{
    public GameObject statsMenuRoot;  // <- drag HUD_Canvas/StatsMenuRoot ici

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame) Toggle();
#else
        if (Input.GetKeyDown(KeyCode.C)) Toggle();
#endif
    }

    void Toggle()
    {
        if (!statsMenuRoot) return;
        bool show = !statsMenuRoot.activeSelf;
        statsMenuRoot.SetActive(show);
        // Optionnel : construire/rafraîchir à l'ouverture
        var menu = statsMenuRoot.GetComponent<PartyStatsMenuUI>();
        if (show) menu?.Show(); else menu?.Hide();
    }
}
