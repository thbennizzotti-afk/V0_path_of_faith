// Assets/App/StartOptions.cs
namespace PathOfFaith.App
{
    public static class StartOptions
    {
        public static bool LoadOnStart { get; private set; }
        public static string Slot { get; private set; }

        public static void NewGame()
        {
            LoadOnStart = false;
            Slot = null;
        }

        public static void RequestLoad(string slotId)
        {
            LoadOnStart = true;
            Slot = slotId;
        }

        public static string ConsumeSlot()
        {
            var s = Slot;
            LoadOnStart = false;
            Slot = null;
            return s;
        }
    }
}
