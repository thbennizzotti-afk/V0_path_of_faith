// Assets/App/StartOptions.cs
namespace PathOfFaith.App
{
    /// <summary>
    /// Transporte l’intention de chargement entre le Main Menu et la scène de jeu.
    /// Aucune logique d’UI ni d’IO ici.
    /// </summary>
    public static class StartOptions
    {
        public static bool HasPendingLoad => !string.IsNullOrEmpty(PendingSlot);
        public static string PendingSlot { get; private set; }

        public static void RequestLoad(string slotId)
        {
            PendingSlot = slotId;
        }

        public static string Consume()
        {
            var s = PendingSlot;
            PendingSlot = null;
            return s;
        }
    }
}
