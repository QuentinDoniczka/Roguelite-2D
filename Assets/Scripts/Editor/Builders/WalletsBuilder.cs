using RogueliteAutoBattler.Economy;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Builders
{
    internal static class WalletsBuilder
    {
        private const string GoldWalletGameObjectName = "GoldWallet";
        private const string SkillPointWalletGameObjectName = "SkillPointWallet";

        internal static GoldWallet FindOrCreateGoldWallet()
            => FindOrCreateSceneSingleton<GoldWallet>(GoldWalletGameObjectName);

        internal static SkillPointWallet FindOrCreateSkillPointWallet()
            => FindOrCreateSceneSingleton<SkillPointWallet>(SkillPointWalletGameObjectName);

        private static TComponent FindOrCreateSceneSingleton<TComponent>(string gameObjectName)
            where TComponent : Component
        {
            TComponent existing = Object.FindFirstObjectByType<TComponent>(FindObjectsInactive.Include);
            if (existing != null) return existing;

            var hostGo = new GameObject(gameObjectName);
            return hostGo.AddComponent<TComponent>();
        }
    }
}
