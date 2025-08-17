namespace PathOfFaith.Fondation.Core
{
    /// <summary>
    /// Contrat minimal pour piloter le curseur sans dépendre de l'UI concrète.
    /// </summary>
    public interface ICursorService
    {
        void SetDefault();
        void SetExploreMove();
    }
}
