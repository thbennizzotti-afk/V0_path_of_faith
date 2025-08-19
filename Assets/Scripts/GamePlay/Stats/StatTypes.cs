namespace PathOfFaith.Gameplay.Stats
{
    // Attributs primaires éditables par le joueur
    public enum PrimaryStat
    {
        Strength,   // Force
        Agility,    // Agilité
        Intellect,  // Intelligence/Magie
        Vitality    // Vigueur/Constitution
    }

    // Stats “adressables” partout dans le jeu
    public enum StatType
    {
        // Dérivées (calculées via formules)
        HPMax, APMax, MPMax,
        AttackPower, Defense,
        Initiative, CritChance, CritDamageMultiplier,
        MoveRange, ActionPointsPerTurn, MovePointsPerTurn,

        // Exposition optionnelle des primaires (pour HUD/IA)
        Strength, Agility, Intellect, Vitality
    }

    public enum ModType { Flat, PercentAdd, PercentMul }
}
