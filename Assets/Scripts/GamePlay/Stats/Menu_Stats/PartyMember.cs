using UnityEngine;
using PathOfFaith.Gameplay.Stats;

[DisallowMultipleComponent]
public class PartyMember : MonoBehaviour
{
    [Range(0,3)] public int slotIndex = 0;          // 0 à 3
    public CharacterStats stats;                    // auto-détecté si vide

    void Reset() { stats = GetComponent<CharacterStats>(); }
}
