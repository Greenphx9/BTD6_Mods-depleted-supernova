using Assets.Scripts.Models;
using Assets.Scripts.Models.GenericBehaviors;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Abilities;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Simulation;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Assets.Scripts.Utils;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[assembly: MelonColor(ConsoleColor.DarkCyan)]
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
        private static List<string> Names = new List<string>();
        private static List<string> Descriptions = new List<string>();
        private static List<string> UpgradeNames = new List<string>();
        private static List<string> UpgradeDescriptions = new List<string>();

        // DOOM
        private static List<string> allDisplays = new List<string>();
        private static List<SpriteReference> allSprites = new List<SpriteReference>();
        private static List<string> allNames = new List<string>();
        private static List<string> allDescriptions = new List<string>();

        private static T getRandomDisplay<T>(List<T> displayType) {return displayType[random.Next(0, displayType.Count - 1)];}

        // Pre-Title
        public override void OnGameModelLoaded(GameModel model)
        {
            base.OnGameModelLoaded(model);
            int count = 0;
            var towerStop = new Stopwatch();
            towerStop.Start();
            foreach (var tower in model.towers)
            {

                // Paragon fix
                if (tower.isParagon)
                {
                    tower.GetBehavior<ParagonTowerModel>().displayDegreePaths.ForEach(x => TowerDisplays.Add(x.assetPath));
                }
                // Air Displays
                if (tower.HasBehavior<AirUnitModel>())
                {
                    TowerDisplays.Add(tower.GetBehavior<AirUnitModel>().display);
                    allDisplays.Add(tower.GetBehavior<AirUnitModel>().display);
                }
                // Tower Names/Descriptions
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
                foreach (var towerDisplay in tower.GetBehaviors<DisplayModel>())
                {
                    if (!TowerDisplays.Contains(towerDisplay.display) && towerDisplay.display != null)
                    {
                        TowerDisplays.Add(towerDisplay.display);
                        allDisplays.Add(towerDisplay.display);
                        count++;
                    }
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
                        // Spactory Fix
                        if (projectile.HasBehavior<SetSpriteFromPierceModel>())
                        {
                            projectile.GetBehavior<SetSpriteFromPierceModel>().sprites.ToList().ForEach(x =>
                            {
                                ProjectileDisplays.Add(x);
                                allDisplays.Add(x);
                                count++;
                            });
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
            towerStop.Stop();
            string towerElapsed = string.Format("{0:00}.{1:00}", towerStop.Elapsed.TotalSeconds, towerStop.Elapsed.Milliseconds);
            MelonLogger.Msg("Tower Displays: " + towerElapsed);
            Stopwatch upgradeStop = new Stopwatch();
            upgradeStop.Start();
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
            upgradeStop.Stop();
            string upgradeElapsed = string.Format("{0:00}.{1:00}", upgradeStop.Elapsed.TotalSeconds, upgradeStop.Elapsed.Milliseconds);
            MelonLogger.Msg("Upgrade Displays: " + upgradeElapsed);
            Stopwatch bloonStop = new Stopwatch();
            bloonStop.Start();
            // Bloon Displays/Sprites
            foreach (var bloon in model.bloons)
            {
                if (!bloon.isBoss)
                {
                    allDisplays.Add(bloon.display);
                    allSprites.Add(bloon.icon);
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
            bloonStop.Stop();
            string bloonElapsed = string.Format("{0:00}.{1:00}", bloonStop.Elapsed.TotalSeconds, bloonStop.Elapsed.Milliseconds);
            MelonLogger.Msg("Bloon Displays: " + bloonElapsed);
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

        [HarmonyPatch(typeof(Simulation),nameof(Simulation.RoundStart))]
        class roundStart_Patch
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                UpdateBloons();
            }
        }

        public static void UpdateSprites()
        {
            var towerStop = new Stopwatch();
            towerStop.Start();
            foreach (var tower in Game.instance.model.towers)
            {
                // Tower Name
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
                        Game.instance.GetLocalizationManager().textTable[tower.baseId + " Description"] = upgradeDesc;
                    }
                    catch (Exception e) { }
                }
                // Tower Display
                if (enableTowers)
                {
                    string ranDisplay = getRandomDisplay(TowerDisplays);
                    if (spriteHell)
                        ranDisplay = getRandomDisplay(allDisplays);
                    tower.GetBehavior<DisplayModel>().display = ranDisplay;
                    tower.display = ranDisplay;
                    if (tower.GetBehaviors<DisplayModel>().Count > 1)
                    {
                        string ranDisplay2 = getRandomDisplay(TowerDisplays);
                        if (spriteHell)
                            ranDisplay2 = getRandomDisplay(allDisplays);
                        tower.GetBehaviors<DisplayModel>()[1].display = ranDisplay2;
                    }
                    // AirDisplay
                    if (tower.HasBehavior<AirUnitModel>())
                    {
                        var ranDisplay2 = getRandomDisplay(TowerDisplays);
                        if (spriteHell)
                            ranDisplay2 = getRandomDisplay(allDisplays);
                        tower.GetBehavior<AirUnitModel>().display = ranDisplay2;
                    }
                    if (tower.isParagon)
                    {
                        tower.GetBehavior<ParagonTowerModel>().displayDegreePaths.ForEach(x =>
                        {
                            var display = getRandomDisplay(TowerDisplays);
                            if (spriteHell)
                                display = getRandomDisplay(allDisplays);
                            x.assetPath = display;
                        });
                    }
                }
                // Projectile Display
                if (enableProjectiles)
                    foreach (var projectile in tower.GetDescendants<ProjectileModel>().ToList())
                    {
                        // Spactory fix
                        if (projectile.HasBehavior<SetSpriteFromPierceModel>())
                            projectile.RemoveBehavior<SetSpriteFromPierceModel>();
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
            towerStop.Stop();
            string towerElapsed = string.Format("{0:00}.{1:00}", towerStop.Elapsed.TotalSeconds, towerStop.Elapsed.Milliseconds);
            MelonLogger.Msg("Towers updated: " + towerElapsed);
            var upgradeStop = new Stopwatch();
            upgradeStop.Start();
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
            upgradeStop.Stop();
            string upgradeElapsed = string.Format("{0:00}.{1:00}", upgradeStop.Elapsed.TotalSeconds, upgradeStop.Elapsed.Milliseconds);
            MelonLogger.Msg("Towers updated: " + upgradeElapsed);
            Stopwatch bloonStop = new Stopwatch();
            bloonStop.Start();
            // Bloon Displays/Sprites
            UpdateBloons();
            bloonStop.Stop();
            string bloonElapsed = string.Format("{0:00}.{1:00}", bloonStop.Elapsed.TotalSeconds, bloonStop.Elapsed.Milliseconds);
            MelonLogger.Msg("Towers updated: " + bloonElapsed);
        }

        public static void UpdateBloons()
        {
            if (enableBloons)
                foreach (var bloon in Game.instance.model.bloons)
                {
                    if (!bloon.isBoss)
                    {
                        if (!spriteHell)
                        {
                            Tuple<string, SpriteReference> bloonVisual = MoabVisuals[random.Next(0, MoabVisuals.Count - 1)];
                            if (!bloon.isMoab)
                                bloonVisual = BloonVisuals[random.Next(0, BloonVisuals.Count - 1)];
                            else
                                bloonVisual = MoabVisuals[random.Next(0, MoabVisuals.Count - 1)];
                            bloon.display = bloonVisual.Item1;
                            bloon.icon = bloonVisual.Item2;
                        }
                        else
                        {
                            bloon.display = getRandomDisplay(allDisplays);
                            bloon.icon = getRandomDisplay(allSprites);
                        }
                    }
                }
        }
    }
}
