using HarmonyLib;
using JModder;
using MelonLoader;
using MelonLoader.Preferences;
using Unity.Burst.CompilerServices;
using UnityEngine;

[assembly: MelonInfo(typeof(LBAMod.Core), "LBAMod", "1.0.0", "jw11-modder", null)]
[assembly: MelonGame("2point21", "LittleBigAdventureTwinsensQuest")]

namespace LBAMod
{
    public class Core : MelonMod
    {
        public static Core Instance { get; private set; }

        private static MelonPreferences_Category MultiplierFloatCategory;
        private static MelonPreferences_Category MultiplierIntCategory;
        private static MelonPreferences_Category ToggleCategory;

        private static MelonPreferences_Entry<KeyCode> configMenuToggle;

        private static MelonPreferences_Entry<bool> configNoPlayerDamage;
        private static MelonPreferences_Entry<bool> configFreeMagic;
        private static MelonPreferences_Entry<bool> configFreeKash;
        private static MelonPreferences_Entry<bool> configFreeFuel;
        private static MelonPreferences_Entry<bool> configFreeKeys;
        private static MelonPreferences_Entry<bool> configFreeClover;

        private static MelonPreferences_Entry<float> configMoneyMultiplier;
        private static MelonPreferences_Entry<float> configWeaponDamageMultiplier;

        public override void OnInitializeMelon()
        {
            Instance = this;

            MultiplierFloatCategory = MelonPreferences.CreateCategory("FloatMultipliers");
            MultiplierIntCategory = MelonPreferences.CreateCategory("IntMultipliers");
            ToggleCategory = MelonPreferences.CreateCategory("Toggles");

            configNoPlayerDamage = ToggleCategory.CreateEntry<bool>("configNoPlayerDamage", false, "No damage to player character");
            configFreeMagic = ToggleCategory.CreateEntry<bool>("configFreeMagic", false, "Don't spend magic");
            configFreeKash = ToggleCategory.CreateEntry<bool>("configFreeKash", false, "Don't spend money");
            configFreeFuel = ToggleCategory.CreateEntry<bool>("configFreeFuel", false, "Don't spend fuel");
            //configFreeKeys = ToggleCategory.CreateEntry<bool>("configFreeKeys", false, "Don't spend keys");
            configFreeClover = ToggleCategory.CreateEntry<bool>("configFreeClover", false, "Don't spend clovers");

            configWeaponDamageMultiplier = MultiplierFloatCategory.CreateEntry<float>("configWeaponDamageMultiplier", 1f, "Player damage multiplier", validator: new ValueRange<float>(1f, 20f));
            configMoneyMultiplier = MultiplierFloatCategory.CreateEntry<float>("configMoneyMultiplier", 1f, "Money income multiplier", validator: new ValueRange<float>(1f, 20f));

            JMod.Init(Instance);
            configMenuToggle = JMod.configMenuToggle;
            JMod.Log("Initialized.");
        }
        public override void OnUpdate()
        {
            if (Event.current != null)
                if (Event.current.keyCode == configMenuToggle.Value && Event.current.type == EventType.KeyDown)
                    JMod.SwitchMenu(false);

            if (Event.current != null)
                if (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyDown)
                    JMod.SwitchMenu(true);
        }

        public override void OnGUI()
        {
            JMod.ShowMenu();
        }

        // configNoPlayerDamage
        // configWeaponDamageMultiplier

        [HarmonyPatch(typeof(Character), nameof(Character.Hit))]
        class CharacterHitPatch1
        {
            static bool Prefix(ref Character hitter, ref int damages, ref Character __instance, ref MovingObject ___m_moving)
            {
                if (__instance.GetMoving().IsPlayer() && configNoPlayerDamage.Value)
                {
                    damages = 0;
                    __instance.m_lifePoints = 50;
                }    
                if (hitter.GetMoving().IsPlayer() && configWeaponDamageMultiplier.Value > 1)
                {
                    damages = Mathf.RoundToInt(damages * configWeaponDamageMultiplier.Value);
                }    
                return true;
            }

        }

        // configFreeMagic

        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.UseMagicPoints))]
        class UseMagicPointsPatch1
        {
            static bool Prefix(ref bool __result)
            {
                if (!configFreeMagic.Value)
                    return true;
                __result = true;
                return false;
            }
        }

        // configFreeFuel

        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.UseFuel))]
        class UseFuelPatch1
        {
            static bool Prefix(ref bool __result)
            {
                if (!configFreeFuel.Value)
                    return true;
                __result = true;
                return false;
            }
        }

        // configFreeKeys

        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.UseLittleKey))]
        class UseLittleKeyPatch1
        {
            static bool Prefix(ref bool __result)
            {
                if (!configFreeKeys.Value)
                    return true;
                __result = true;
                return false;
            }
        }

        // configFreeClover

        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.UseClover))]
        class UseCloverPatch1
        {
            static bool Prefix(ref bool __result)
            {
                if (!configFreeClover.Value)
                    return true;
                GameHandler.FullPoints();
                __result = true;
                return false;
            }
        }

        // configMoneyMultiplier

        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.AddKash))]
        class AddKashPatch1
        {
            static bool Prefix(ref int count)
            {
                if (configMoneyMultiplier.Value <= 1 && count > 0)
                    return true;
                count = Mathf.RoundToInt(count * configMoneyMultiplier.Value);
                return true;
            }

        }
        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.UseKash))]
        class UseKashPatch1
        {
            static bool Prefix(ref bool __result)
            {
                if (!configFreeKash.Value)
                    return true;
                __result = true;
                return false;
            }

        }
    }
}