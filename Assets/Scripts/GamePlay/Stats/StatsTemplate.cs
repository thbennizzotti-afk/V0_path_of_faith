using System;
using UnityEngine;

namespace PathOfFaith.Gameplay.Stats
{
    [CreateAssetMenu(menuName = "PathOfFaith/Stats/Template", fileName = "StatsTemplate")]
    public class StatsTemplate : ScriptableObject
    {
        [Header("Attributs de base (niveau 1, avant points joueurs)")]
        public int strength   = 5;
        public int agility    = 5;
        public int intellect  = 5;
        public int vitality   = 5;

        [Header("Croissance automatique par niveau (optionnel)")]
        public int strPerLevel = 0;
        public int agiPerLevel = 0;
        public int intPerLevel = 0;
        public int vitPerLevel = 1;

        [Header("Points à distribuer par niveau (joueur)")]
        public int attributePointsOnLevelUp = 3;
    }
}
