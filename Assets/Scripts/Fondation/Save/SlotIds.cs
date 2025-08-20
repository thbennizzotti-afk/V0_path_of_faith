namespace PathOfFaith.Save
{
    public static class SlotIds
    {
        public static readonly string[] All = { "slot1", "slot2", "slot3" };
        public static string FromIndex(int oneBased) => oneBased switch
        {
            1 => "slot1",
            2 => "slot2",
            3 => "slot3",
            _ => null
        };
    }
}
