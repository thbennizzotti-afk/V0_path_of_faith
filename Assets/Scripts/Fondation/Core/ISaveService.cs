namespace PathOfFaith.Fondation.Core
{
    /// <summary>Contrat public du service de sauvegarde.</summary>
    public interface ISaveService
    {
        void Save(string slot);
        bool Load(string slot);
        bool Exists(string slot);
        void Delete(string slot);
        System.Collections.Generic.IEnumerable<string> ListExistingSlots();
        (bool ok, System.DateTime savedAt, string scene, int version) GetInfo(string slot);
    }
}
