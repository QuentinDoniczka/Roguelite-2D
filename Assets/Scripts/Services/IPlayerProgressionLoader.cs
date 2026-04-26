namespace RogueliteAutoBattler.Services
{
    public interface IPlayerProgressionLoader
    {
        void Load();
        void Save();
        void ResetAll();
    }
}
