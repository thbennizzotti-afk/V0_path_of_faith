using UnityEngine;
using PathOfFaith.Gameplay.Stats;

public class PrintStatsOnStart : MonoBehaviour {
    void Start() {
        var s = GetComponent<CharacterStats>();
        Debug.Log($"[{name}] Lvl {s.level}  HPMax={s.GetStat(StatType.HPMax)}  APMax={s.GetStat(StatType.APMax)}  MPMax={s.GetStat(StatType.MPMax)}  ATK={s.GetStat(StatType.AttackPower)}  DEF={s.GetStat(StatType.Defense)}  INIT={s.GetStat(StatType.Initiative)}  CRIT={(s.GetStat(StatType.CritChance)*100f):0}% x{s.GetStat(StatType.CritDamageMultiplier):0.00}");
    }
}
