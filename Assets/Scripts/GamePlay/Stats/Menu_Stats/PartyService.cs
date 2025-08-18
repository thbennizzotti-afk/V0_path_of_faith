using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PartyService : MonoBehaviour
{
    readonly List<PartyMember> _members = new();
    public ReadOnlyCollection<PartyMember> Members => _members.AsReadOnly();

    void Awake() { RebuildFromScene(); }

    public void RebuildFromScene()
    {
        _members.Clear();

        // Nouveau finder (Unity 6) — pas obsolète
        var found = Object.FindObjectsByType<PartyMember>(FindObjectsSortMode.None);

        _members.AddRange(found
            .OrderBy(m => m.slotIndex)
            .ThenBy(m => m.gameObject.scene.buildIndex));
    }

    public PartyMember GetBySlot(int slot)
        => _members.FirstOrDefault(m => m.slotIndex == slot);
}
