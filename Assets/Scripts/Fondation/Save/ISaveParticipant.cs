namespace PathOfFaith.Save
{
    /// <summary>Un système qui participe à la capture/application de l'état.</summary>
    public interface ISaveParticipant
    {
        void Capture(GameSave save);
        void Apply(GameSave save);
    }
}
