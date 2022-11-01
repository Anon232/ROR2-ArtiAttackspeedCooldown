using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using BepInEx.Configuration;
using System.Xml;
using System.Runtime.CompilerServices;

namespace ArtiAttackspeedCooldown
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(RecalculateStatsAPI))]
    public class ArtiAttackspeedCooldown : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Anon232";
        public const string PluginName = "ArtiAttackspeedCooldown";
        public const string PluginVersion = "1.1.0";

        private static ConfigEntry<int> reductionPercent { get; set; }
        private static ConfigEntry<bool> onlyPrimary { get; set; }

        private static float fReductionPct { get; set; }

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            //set config
            reductionPercent = base.Config.Bind<int>("Arti AttackSpeed Cooldown", "Reduction Amount Percent", 75, "What percentage of attackspeed to use to reduce cooldowns by. Default is 75%.");
            onlyPrimary = base.Config.Bind<bool>("Arti AttackSpeed Cooldown", "Only Affects Primary Skill", false, "Check whether this should only affect Arti's primary skill. Default is false.");


            //setup risk of options.
            if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                SetupRiskOfOptions();
            }

            reductionPercent.Value = (reductionPercent.Value > 0  && reductionPercent.Value <= 100) ? reductionPercent.Value : 0;

            Log.LogInfo("Reduction Config amount: " + reductionPercent.Value);

            fReductionPct = reductionPercent.Value > 0 ? ((float)(reductionPercent.Value) / 100) : 0f;

            Log.LogInfo("Reduction amount: " + fReductionPct.ToString("0.00"));

            On.RoR2.CharacterBody.RecalculateStats += delegate (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody body)
            {
                orig.Invoke(body);
                ModifyArtiCooldown(body);
            };


            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        private static void ModifyArtiCooldown(CharacterBody body)
        {
            var artiPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageBody.prefab").WaitForCompletion();
            var artiBody = artiPrefab.GetComponent<CharacterBody>();

            if(artiBody.bodyIndex == body.bodyIndex)
            {
                if (body.skillLocator.primary)
                {
                    body.skillLocator.primary.cooldownScale /= (body.attackSpeed * fReductionPct);
                }
                if ( !onlyPrimary.Value && body.skillLocator.secondary)
                {
                    body.skillLocator.secondary.cooldownScale /= (body.attackSpeed * fReductionPct);
                }
                if ( !onlyPrimary.Value && body.skillLocator.utility)
                {
                    body.skillLocator.utility.cooldownScale /= (body.attackSpeed * fReductionPct);
                }
                if ( !onlyPrimary.Value && body.skillLocator.special)
                {
                    body.skillLocator.special.cooldownScale /= (body.attackSpeed * fReductionPct);
                }
            }
        }

        private static void SetupRiskOfOptions()
        {
            ModSettingsManager.SetModDescription("Configuration for ArtiAttackSpeedCooldown.");
            ModSettingsManager.AddOption(new IntSliderOption(reductionPercent, new IntSliderConfig() { min = 0, max = 100, formatString = "{0:0}%" }));
            ModSettingsManager.AddOption(new CheckBoxOption(onlyPrimary));
        }
    }
}
