using Assets.Scripts.Models;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Models.Towers.Filters;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Assets.Scripts.Unity.UI_New.Popups;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.Models.Towers.TowerModel;

[assembly: MelonColor(ConsoleColor.DarkCyan)]
[assembly: MelonInfo(typeof(RandomStats.Main), "Random Stats", "1.0", "DepletedNova")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace RandomStats
{
    public class Main : BloonsTD6Mod
    {
        public static Il2CppSystem.Random random = new Il2CppSystem.Random();

        public static List<(int, int)> costRange = new List<(int, int)>()
        {
            (200, 1500),
            (500, 2500),
            (2000, 5000),
            (3500, 15000),
            (20000, 100000),
            (350000, 650000)
        };

        public static List<string> towerSets = new List<string>()
        {
            "Primary",
            "Military",
            "Magic",
            "Support"
        };

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            MelonLogger.Msg("Random Stats began.");
        }

        public override void OnGameModelLoaded(GameModel model)
        {
            base.OnGameModelLoaded(model);

            foreach (var upgrade in model.upgrades)
            {
                if (upgrade.tier < 5)
                {
                    upgrade.cost = random.Next(costRange[upgrade.tier].Item1, costRange[upgrade.tier].Item2);
                } 
                else if (upgrade.IsParagon)
                {
                    upgrade.cost = random.Next(costRange[5].Item1, costRange[5].Item2);
                }
            }

            foreach (var tower in model.towers)
            {
                tower.footprint = new FootprintModel("Footprint_", random.Next(2) == 0, random.Next(2) == 0, random.Next(2) == 0);
                var nAreas = new List<AreaType>()
                {
                    AreaType.land,
                    AreaType.track,
                    AreaType.unplaceable,
                    AreaType.water,
                };
                var ranArea = random.Next(1, nAreas.Count);
                var areas = new UnhollowerBaseLib.Il2CppStructArray<AreaType>(ranArea);
                for (int x = 0; x < ranArea; x++)
                {
                    var area = nAreas[random.Next(0, nAreas.Count)];
                    areas[x] = area;
                    nAreas.Remove(area);
                }
                tower.areaTypes = areas;
                tower.isGlobalRange = random.Next(2) == 0;
                var range = random.Next(15, 80);
                if (tower.range > 80)
                    range = random.Next(80, 150);
                if (range > 120)
                {
                    range = 10;
                    tower.isGlobalRange = true;
                }
                tower.range = range;
                foreach (var attackModel in tower.GetDescendants<AttackModel>().ToList())
                    attackModel.range = range;
                if (!tower.IsHero())
                    tower.towerSet = towerSets[random.Next(towerSets.Count)];
                foreach (var weaponModel in tower.GetDescendants<WeaponModel>().ToList())
                {
                    weaponModel.rate = random.Next(50, 100) / random.Next(50, 100);
                    weaponModel.fireWithoutTarget = random.Next(3) == 0;
                    weaponModel.fireBetweenRounds = random.Next(3) == 0;
                }
                foreach (var invisibleModel in tower.GetDescendants<FilterInvisibleModel>().ToList())
                    invisibleModel.isActive = random.Next(2) == 0;
            }
        }
    }
}
