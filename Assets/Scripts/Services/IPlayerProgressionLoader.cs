using System;

namespace RogueliteAutoBattler.Services
{
    public interface IPlayerProgressionLoader : IDisposable
    {
        void Load();
        void Save();
        void ResetAll();
    }
}
