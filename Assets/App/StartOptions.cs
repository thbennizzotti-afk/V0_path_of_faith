using PathOfFaith.Fondation.Core;

namespace PathOfFaith.App
{
    /// <summary>Choix du slot et intention New/Load au lancement.</summary>
    public static class StartOptions
    {
        public static bool LoadOnStart { get; private set; }
        public static string CurrentSlot { get; private set; } = "slot1";

        public static void NewGame(string slot)
        {
            LoadOnStart = false;
            CurrentSlot = slot;
            SaveContext.CurrentSlot = slot;   // <- met à jour le contexte visible par l’UI
        }

        public static void LoadGame(string slot)
        {
            LoadOnStart = true;
            CurrentSlot = slot;
            SaveContext.CurrentSlot = slot;   // <- idem
        }
    }
}
