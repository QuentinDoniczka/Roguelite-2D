using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    internal static class DamageNumberSettingsPersistence
    {
        private const string HasSavedKey = "DmgNum_HasSaved";
        private const string FontSizeKey = "DmgNum_FontSize";
        private const string LifetimeKey = "DmgNum_Lifetime";
        private const string SpawnOffsetYKey = "DmgNum_SpawnOffsetY";
        private const string AllyColorRKey = "DmgNum_AllyR";
        private const string AllyColorGKey = "DmgNum_AllyG";
        private const string AllyColorBKey = "DmgNum_AllyB";
        private const string AllyColorAKey = "DmgNum_AllyA";
        private const string EnemyColorRKey = "DmgNum_EnemyR";
        private const string EnemyColorGKey = "DmgNum_EnemyG";
        private const string EnemyColorBKey = "DmgNum_EnemyB";
        private const string EnemyColorAKey = "DmgNum_EnemyA";

        internal static void Save(DamageNumberConfig config)
        {
            PlayerPrefs.SetFloat(FontSizeKey, config.FontSize);
            PlayerPrefs.SetFloat(LifetimeKey, config.Lifetime);
            PlayerPrefs.SetFloat(SpawnOffsetYKey, config.SpawnOffsetY);
            PlayerPrefs.SetFloat(AllyColorRKey, config.AllyDamageColor.r);
            PlayerPrefs.SetFloat(AllyColorGKey, config.AllyDamageColor.g);
            PlayerPrefs.SetFloat(AllyColorBKey, config.AllyDamageColor.b);
            PlayerPrefs.SetFloat(AllyColorAKey, config.AllyDamageColor.a);
            PlayerPrefs.SetFloat(EnemyColorRKey, config.EnemyDamageColor.r);
            PlayerPrefs.SetFloat(EnemyColorGKey, config.EnemyDamageColor.g);
            PlayerPrefs.SetFloat(EnemyColorBKey, config.EnemyDamageColor.b);
            PlayerPrefs.SetFloat(EnemyColorAKey, config.EnemyDamageColor.a);
            PlayerPrefs.SetInt(HasSavedKey, 1);
            PlayerPrefs.Save();
        }

        internal static void Load(DamageNumberConfig config)
        {
            if (PlayerPrefs.GetInt(HasSavedKey, 0) != 1)
                return;

            config.FontSize = PlayerPrefs.GetFloat(FontSizeKey, config.FontSize);
            config.Lifetime = PlayerPrefs.GetFloat(LifetimeKey, config.Lifetime);
            config.SpawnOffsetY = PlayerPrefs.GetFloat(SpawnOffsetYKey, config.SpawnOffsetY);
            config.AllyDamageColor = new Color(
                PlayerPrefs.GetFloat(AllyColorRKey, config.AllyDamageColor.r),
                PlayerPrefs.GetFloat(AllyColorGKey, config.AllyDamageColor.g),
                PlayerPrefs.GetFloat(AllyColorBKey, config.AllyDamageColor.b),
                PlayerPrefs.GetFloat(AllyColorAKey, config.AllyDamageColor.a));
            config.EnemyDamageColor = new Color(
                PlayerPrefs.GetFloat(EnemyColorRKey, config.EnemyDamageColor.r),
                PlayerPrefs.GetFloat(EnemyColorGKey, config.EnemyDamageColor.g),
                PlayerPrefs.GetFloat(EnemyColorBKey, config.EnemyDamageColor.b),
                PlayerPrefs.GetFloat(EnemyColorAKey, config.EnemyDamageColor.a));
        }

        internal static void DeleteAll()
        {
            PlayerPrefs.DeleteKey(HasSavedKey);
            PlayerPrefs.DeleteKey(FontSizeKey);
            PlayerPrefs.DeleteKey(LifetimeKey);
            PlayerPrefs.DeleteKey(SpawnOffsetYKey);
            PlayerPrefs.DeleteKey(AllyColorRKey);
            PlayerPrefs.DeleteKey(AllyColorGKey);
            PlayerPrefs.DeleteKey(AllyColorBKey);
            PlayerPrefs.DeleteKey(AllyColorAKey);
            PlayerPrefs.DeleteKey(EnemyColorRKey);
            PlayerPrefs.DeleteKey(EnemyColorGKey);
            PlayerPrefs.DeleteKey(EnemyColorBKey);
            PlayerPrefs.DeleteKey(EnemyColorAKey);
            PlayerPrefs.Save();
        }
    }
}
