// Assets/Scripts/Fondation/Save/ISaveParticipant.cs
namespace PathOfFaith.Save
{
    public interface ISaveParticipant
    {
        void Capture(GameSave save);
        void Apply(GameSave save);
    }
}
