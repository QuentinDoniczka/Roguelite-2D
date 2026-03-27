using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    internal static class DamageNumberSettingsPersistence
    {
        private const string HAS_SAVED_KEY = "DmgNum_HasSaved";
        private const string FONT_SIZE_KEY = "DmgNum_FontSize";
        private const string LIFETIME_KEY = "DmgNum_Lifetime";
        private const string SLIDE_DISTANCE_KEY = "DmgNum_SlideDistance";
        private const string SPAWN_OFFSET_Y_KEY = "DmgNum_SpawnOffsetY";
        private const string ALLY_COLOR_R_KEY = "DmgNum_AllyR";
        private const string ALLY_COLOR_G_KEY = "DmgNum_AllyG";
        private const string ALLY_COLOR_B_KEY = "DmgNum_AllyB";
        private const string ALLY_COLOR_A_KEY = "DmgNum_AllyA";
        private const string ENEMY_COLOR_R_KEY = "DmgNum_EnemyR";
        private const string ENEMY_COLOR_G_KEY = "DmgNum_EnemyG";
        private const string ENEMY_COLOR_B_KEY = "DmgNum_EnemyB";
        private const string ENEMY_COLOR_A_KEY = "DmgNum_EnemyA";

        internal static void Save(DamageNumberConfig config)
        {
            PlayerPrefs.SetFloat(FONT_SIZE_KEY, config.FontSize);
            PlayerPrefs.SetFloat(LIFETIME_KEY, config.Lifetime);
            PlayerPrefs.SetFloat(SLIDE_DISTANCE_KEY, config.SlideDistance);
            PlayerPrefs.SetFloat(SPAWN_OFFSET_Y_KEY, config.SpawnOffsetY);
            PlayerPrefs.SetFloat(ALLY_COLOR_R_KEY, config.AllyDamageColor.r);
            PlayerPrefs.SetFloat(ALLY_COLOR_G_KEY, config.AllyDamageColor.g);
            PlayerPrefs.SetFloat(ALLY_COLOR_B_KEY, config.AllyDamageColor.b);
            PlayerPrefs.SetFloat(ALLY_COLOR_A_KEY, config.AllyDamageColor.a);
            PlayerPrefs.SetFloat(ENEMY_COLOR_R_KEY, config.EnemyDamageColor.r);
            PlayerPrefs.SetFloat(ENEMY_COLOR_G_KEY, config.EnemyDamageColor.g);
            PlayerPrefs.SetFloat(ENEMY_COLOR_B_KEY, config.EnemyDamageColor.b);
            PlayerPrefs.SetFloat(ENEMY_COLOR_A_KEY, config.EnemyDamageColor.a);
            PlayerPrefs.SetInt(HAS_SAVED_KEY, 1);
            PlayerPrefs.Save();
        }

        internal static void Load(DamageNumberConfig config)
        {
            if (PlayerPrefs.GetInt(HAS_SAVED_KEY, 0) != 1)
                return;

            config.FontSize = PlayerPrefs.GetFloat(FONT_SIZE_KEY, config.FontSize);
            config.Lifetime = PlayerPrefs.GetFloat(LIFETIME_KEY, config.Lifetime);
            config.SlideDistance = PlayerPrefs.GetFloat(SLIDE_DISTANCE_KEY, config.SlideDistance);
            config.SpawnOffsetY = PlayerPrefs.GetFloat(SPAWN_OFFSET_Y_KEY, config.SpawnOffsetY);
            config.AllyDamageColor = new Color(
                PlayerPrefs.GetFloat(ALLY_COLOR_R_KEY, config.AllyDamageColor.r),
                PlayerPrefs.GetFloat(ALLY_COLOR_G_KEY, config.AllyDamageColor.g),
                PlayerPrefs.GetFloat(ALLY_COLOR_B_KEY, config.AllyDamageColor.b),
                PlayerPrefs.GetFloat(ALLY_COLOR_A_KEY, config.AllyDamageColor.a));
            config.EnemyDamageColor = new Color(
                PlayerPrefs.GetFloat(ENEMY_COLOR_R_KEY, config.EnemyDamageColor.r),
                PlayerPrefs.GetFloat(ENEMY_COLOR_G_KEY, config.EnemyDamageColor.g),
                PlayerPrefs.GetFloat(ENEMY_COLOR_B_KEY, config.EnemyDamageColor.b),
                PlayerPrefs.GetFloat(ENEMY_COLOR_A_KEY, config.EnemyDamageColor.a));
        }

        internal static void DeleteAll()
        {
            PlayerPrefs.DeleteKey(HAS_SAVED_KEY);
            PlayerPrefs.DeleteKey(FONT_SIZE_KEY);
            PlayerPrefs.DeleteKey(LIFETIME_KEY);
            PlayerPrefs.DeleteKey(SLIDE_DISTANCE_KEY);
            PlayerPrefs.DeleteKey(SPAWN_OFFSET_Y_KEY);
            PlayerPrefs.DeleteKey(ALLY_COLOR_R_KEY);
            PlayerPrefs.DeleteKey(ALLY_COLOR_G_KEY);
            PlayerPrefs.DeleteKey(ALLY_COLOR_B_KEY);
            PlayerPrefs.DeleteKey(ALLY_COLOR_A_KEY);
            PlayerPrefs.DeleteKey(ENEMY_COLOR_R_KEY);
            PlayerPrefs.DeleteKey(ENEMY_COLOR_G_KEY);
            PlayerPrefs.DeleteKey(ENEMY_COLOR_B_KEY);
            PlayerPrefs.DeleteKey(ENEMY_COLOR_A_KEY);
            PlayerPrefs.Save();
        }
    }
}
