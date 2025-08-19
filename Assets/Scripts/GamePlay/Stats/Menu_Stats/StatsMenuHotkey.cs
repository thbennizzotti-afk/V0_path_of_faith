using UnityEngine;

public class StatsMenuHotkey : MonoBehaviour
{
    [SerializeField] KeyCode hotkey = KeyCode.C;
    [SerializeField] PartyStatsMenuUI menu; // assigne dans l’Inspector

    void Update()
    {
        if (!menu) return;
        if (Input.GetKeyDown(hotkey))
            menu.Toggle();
    }
}
