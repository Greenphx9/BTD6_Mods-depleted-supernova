using Assets.Scripts.Models;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Utils;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[assembly: MelonInfo(typeof(RandomSprites.Main), "Random Displays", "1.0", "DepletedNova")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace RandomSprites
{
    public class Main : BloonsTD6Mod
    {
        // Refresh options
        public static ModSettingBool spriteHell = new ModSettingBool(false) { displayName = "Display Hell" };
        public static ModSettingBool spriteUpdate = new ModSettingBool(false) { displayName = "Update displays", IsButton = true };
        // Enable options
        public static ModSettingBool enableTowers = new ModSettingBool(true) { displayName = "Enable Tower scrambler?" };
        public static ModSettingBool enableProjectiles = new ModSettingBool(true) { displayName = "Enable Projectile scrambler?" };
        public static ModSettingBool enableBloons = new ModSettingBool(true) { displayName = "Enable Bloon scrambler?" };
        public static ModSettingBool enableIcons = new ModSettingBool(true) { displayName = "Enable Icon scrambler?" };
        public static ModSettingBool enableUpgrades = new ModSettingBool(true) { displayName = "Enable Upgrade Icon scrambler?" };
        public static ModSettingBool enableName = new ModSettingBool(true) { displayName = "Enable Name/Description scrambler?" };
        public static ModSettingBool enableUpgradeText = new ModSettingBool(true) { displayName = "Enable Upgrade Name scrambler?" };

        // Randomizer
        private static Il2CppSystem.Random random = new Il2CppSystem.Random();

        // Tower Displays
        private static List<string> TowerDisplays = new List<string>();
        private static List<string> ProjectileDisplays = new List<string>();
        private static List<string> attackDisplays = new List<string>();

        // Bloon Displays/Sprites
        private static Dictionary<int, Tuple<string, SpriteReference>> BloonVisuals = new Dictionary<int, Tuple<string, SpriteReference>>();
        private static Dictionary<int, Tuple<string, SpriteReference>> MoabVisuals = new Dictionary<int, Tuple<string, SpriteReference>>();

        // Icon Displays
        private static List<SpriteReference> Portraits = new List<SpriteReference>();
        private static List<SpriteReference> Icons = new List<SpriteReference>();
        private static List<SpriteReference> InstaIcons = new List<SpriteReference>();
        private static List<SpriteReference> UpgradeIcons = new List<SpriteReference>();
        private static List<SpriteReference> AbilityIcons = new List<SpriteReference>();

        // TextTables
        private List<string> Names = new List<string>();
        private List<string> Descriptions = new List<string>();
        private List<string> UpgradeNames = new List<string>();
        private List<string> UpgradeDescriptions = new List<string>();

        // DOOM
        private static List<string> allDisplays = new List<string>();
        private static List<SpriteReference> allSprites = new List<SpriteReference>();
        private static Dictionary<int, Tuple<string, SpriteReference>> allBloons = new Dictionary<int, Tuple<string, SpriteReference>>();
        private static List<string> allNames = new List<string>();
        private static List<string> allDescriptions = new List<string>();

        private static T getRandomDisplay<T>(List<T> displayType) {return displayType[random.Next(0, displayType.Count - 1)];}

        // Pre-Title
        public override void OnGameModelLoaded(GameModel model)
        {
            base.OnGameModelLoaded(model);
            int count = 0;
            foreach (var tower in model.towers)
            {
                if (!tower.IsHero() && !Names.Contains(tower.baseId))
                {
                    try
                    {
                        Names.Add(Game.instance.GetLocalizationManager().textTable[tower.baseId]);
                        Descriptions.Add(Game.instance.GetLocalizationManager().textTable[tower.baseId + " Description"]);
                        allNames.Add(Game.instance.GetLocalizationManager().textTable[tower.baseId]);
                        allDescriptions.Add(Game.instance.GetLocalizationManager().textTable[tower.baseId + " Description"]);
                        count += 2;
                    } catch (Exception e) { }
                }
                // Tower Displays
                if (!TowerDisplays.Contains(tower.GetBehavior<DisplayModel>().display) && tower.GetBehavior<DisplayModel>().display != null)
                {
                    TowerDisplays.Add(tower.GetBehavior<DisplayModel>().display);
                    allDisplays.Add(tower.GetBehavior<DisplayModel>().display);
                    count++;
                }
                // Projectile Displays
                tower.GetDescendants<ProjectileModel>().ForEach(delegate (ProjectileModel projectile)
                {
                    if (projectile.HasBehavior<DisplayModel>())
                    {
                        if (!ProjectileDisplays.Contains(projectile.display) && projectile.display != null)
                        {
                            ProjectileDisplays.Add(projectile.display);
                            allDisplays.Add(projectile.display);
                            count++;
                        }
                    }
                });
                // Attack Model Displays
                foreach (var attackModel in tower.GetBehaviors<AttackModel>())
                    if (attackModel.HasBehavior<DisplayModel>())
                    {
                        if (!attackDisplays.Contains(attackModel.GetBehavior<DisplayModel>().display) && attackModel.GetBehavior<DisplayModel>().display != null)
                        {
                            attackDisplays.Add(attackModel.GetBehavior<DisplayModel>().display);
                            allDisplays.Add(attackModel.GetBehavior<DisplayModel>().display);
                            count++;
                        }
                    }
                // Ability Model Sprites
                tower.GetDescendants<AbilityModel>().ForEach(delegate (AbilityModel ability)
                {
                    if (ability?.icon != null)
                    {
                        if (!AbilityIcons.Contains(ability.icon) && ability.icon?.GUID != null)
                        {
                            AbilityIcons.Add(ability.icon);
                            allSprites.Add(ability.icon);
                            count++;
                        }
                    }
                });
                // Basic Sprites
                if (!Icons.Contains(tower.icon) && tower.icon?.GUID != null)
                {
                    Icons.Add(tower.icon);
                    allSprites.Add(tower.icon);
                    count++;
                }
                if (!Portraits.Contains(tower.portrait) && tower.portrait?.GUID != null)
                {
                    Portraits.Add(tower.portrait);
                    allSprites.Add(tower.portrait);
                    count++;
                }
                if (!InstaIcons.Contains(tower.instaIcon) && tower.instaIcon?.GUID != null)
                {
                    InstaIcons.Add(tower.instaIcon);
                    allSprites.Add(tower.instaIcon);
                    count++;
                }
            }
            
            foreach (var upgrade in model.upgrades)
            {
                // Upgrade Names/Descriptions
                try
                {
                    UpgradeNames.Add(Game.instance.GetLocalizationManager().textTable[upgrade.name]);
                    UpgradeDescriptions.Add(Game.instance.GetLocalizationManager().textTable[upgrade.name+" Description"]);
                    allNames.Add(Game.instance.GetLocalizationManager().textTable[upgrade.name]);
                    allDescriptions.Add(Game.instance.GetLocalizationManager().textTable[upgrade.name + " Description"]);
                    count += 2;
                } catch (Exception e) { }
                // Upgrade Sprites
                if (upgrade.icon?.GUID != null)
                {
                    UpgradeIcons.Add(upgrade.icon);
                    allSprites.Add(upgrade.icon);
                }
                count++;
            }
            // Bloon Displays/Sprites
            foreach (var bloon in model.bloons)
            {
                if (!bloon.isBoss)
                {
                    allBloons.Add(allBloons.Count, new Tuple<string, SpriteReference>(bloon.display, bloon.icon));
                    if (bloon.isMoab)
                    {
                        MoabVisuals.Add(MoabVisuals.Count, new Tuple<string, SpriteReference>(bloon.display, bloon.icon));
                    }
                    else
                    {
                        BloonVisuals.Add(BloonVisuals.Count, new Tuple<string, SpriteReference>(bloon.display, bloon.icon));
                    }
                    count += 2;
                }
            }
            MelonLogger.Msg($"Displays gathered: {count}");
        }

        public override void OnTitleScreen()
        {
            base.OnTitleScreen();
            spriteUpdate.OnValueChanged.Add(option =>
            {
                UpdateSprites();
                MelonLogger.Msg("Displays updated");
            });
            spriteHell.OnValueChanged.Add(option =>
            {
                UpdateSprites();
                MelonLogger.Msg("Display Hell activated");
            });
        }

        // Post-Title
        public override void OnMainMenu()
        {
            base.OnMainMenu();
            UpdateSprites();
            MelonLogger.Msg("Displays loaded");
        }

        [HarmonyPatch(typeof(InGame),nameof(InGame.RoundStart))]
        class roundStart_Patch
        {
            [HarmonyPostfix]
            internal static void Prefix()
            {
                UpdateBloons();
            }
        }

        public void UpdateSprites()
        {
            foreach (var tower in Game.instance.model.towers)
            {
                // Tower TextTables
                if (enableName)
                {
                    try
                    {
                        var upgradeName = getRandomDisplay(Names);
                        var upgradeDesc = getRandomDisplay(Descriptions);
                        if (spriteHell)
                        {
                            upgradeName = getRandomDisplay(allNames);
                            upgradeDesc = getRandomDisplay(allDescriptions);
                        }
                        Game.instance.GetLocalizationManager().textTable[tower.baseId] = upgradeName;
                        Game.instance.GetLocalizationManager().textTable[tower.baseId+" Description"] = upgradeDesc;
                    }
                    catch (Exception e) { }
                }
                // Tower Display
                if (enableTowers)
                {
                    string ranDisplay = getRandomDisplay(TowerDisplays);
                    tower.GetBehavior<DisplayModel>().display = ranDisplay;
                    tower.display = ranDisplay;
                }
                // Projectile Display
                if (enableProjectiles)
                    foreach (var projectile in tower.GetDescendants<ProjectileModel>().ToList())
                    {
                        if (projectile.HasBehavior<DisplayModel>())
                        {
                            string ranProjectile = getRandomDisplay(ProjectileDisplays);
                            if (spriteHell)
                                ranProjectile = getRandomDisplay(allDisplays);
                            projectile.display = ranProjectile;
                            projectile.GetBehavior<DisplayModel>().display = ranProjectile;
                        }
                    }
                // WeaponModel Display
                if (enableTowers)
                    foreach (var attackModel in tower.GetBehaviors<AttackModel>())
                    {
                        string ranAttack = getRandomDisplay(attackDisplays);
                        if (spriteHell)
                            ranAttack = getRandomDisplay(allDisplays);
                        if (attackModel.HasBehavior<DisplayModel>())
                            attackModel.GetBehavior<DisplayModel>().display = ranAttack;
                    }
                // Ability Sprites
                if (enableIcons)
                {
                    tower.GetDescendants<AbilityModel>().ForEach(delegate (AbilityModel ability)
                    {
                        if (ability?.icon != null)
                        {
                            var abilityIcon = getRandomDisplay(AbilityIcons);
                            if (spriteHell)
                                abilityIcon = getRandomDisplay(allSprites);
                            ability.icon = abilityIcon;
                        }
                    });
                    // Basic Sprites
                    SpriteReference icon = getRandomDisplay(Icons);
                    SpriteReference portrait = getRandomDisplay(Portraits);
                    SpriteReference instaIcon = getRandomDisplay(InstaIcons);
                    if (spriteHell)
                    {
                        icon = getRandomDisplay(allSprites);
                        portrait = getRandomDisplay(allSprites);
                        instaIcon = getRandomDisplay(allSprites);
                    }
                    tower.icon = icon;
                    tower.portrait = portrait;
                    tower.instaIcon = instaIcon;
                }
                
            }
            

            foreach (var upgrade in Game.instance.model.upgrades)
            {
                // Upgrade Text
                if (enableUpgradeText)
                {
                    try
                    {
                        var upgradeName = getRandomDisplay(UpgradeNames);
                        var upgradeDesc = getRandomDisplay(UpgradeDescriptions);
                        if (spriteHell)
                        {
                            upgradeName = getRandomDisplay(allNames);
                            upgradeDesc = getRandomDisplay(allDescriptions);
                        }
                        Game.instance.GetLocalizationManager().textTable[upgrade.name] = upgradeName;
                        Game.instance.GetLocalizationManager().textTable[upgrade.name + " Description"] = upgradeDesc;
                    } catch (Exception e) { }
                }
                // Upgrade Icons
                if (enableUpgrades)
                {
                    var upgradeIcon = getRandomDisplay(UpgradeIcons);
                    if (spriteHell)
                        upgradeIcon = getRandomDisplay(allSprites);
                    upgrade.icon = upgradeIcon;
                }
            }
            // Bloon Displays/Sprites
            UpdateBloons();
        }

        public static void UpdateBloons()
        {
            if (enableBloons)
                foreach (var bloon in Game.instance.model.bloons)
                {
                    if (!bloon.isBoss)
                    {
                        Tuple<string, SpriteReference> bloonVisual = allBloons[random.Next(0, allBloons.Count - 1)];
                        if (!spriteHell)
                        {
                            if (!bloon.isMoab)
                                bloonVisual = BloonVisuals[random.Next(0, BloonVisuals.Count - 1)];
                            else
                                bloonVisual = MoabVisuals[random.Next(0, MoabVisuals.Count - 1)];
                        }
                        bloon.display = bloonVisual.Item1;
                        bloon.icon = bloonVisual.Item2;
                    }
                    
                }
        }
    }
}
