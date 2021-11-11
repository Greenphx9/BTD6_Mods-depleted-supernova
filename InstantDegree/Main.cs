using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Popups;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using System;

[assembly: MelonInfo(typeof(InstantDegree.Main), "Instant Degrees", "1.0", "DepletedNova")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace InstantDegree
{
    public class Main : BloonsTD6Mod
    {
        public static ModSettingBool Enabled = new ModSettingBool(true) { displayName = "Enabled?" };
        public static ModSettingBool AskForDegree = new ModSettingBool(true) { displayName = "Dynamic Degree?" };

        [HarmonyPatch(typeof(UnityToSimulation), nameof(UnityToSimulation.UpgradeTowerParagon))]
        class UpgradeToParagon
        {
            [HarmonyPostfix]
            internal static void Postfix(int id)
            {
                foreach (var simTower in InGame.instance.UnityToSimulation.GetAllTowers())
                {
                    if (simTower.id == id)
                    {
                        var tower = simTower.tower.GetTowerBehavior<ParagonTower>();
                        var degree = tower.GetCurrentDegree()+1;
                        ParagonTower.InvestmentInfo info = tower.investmentInfo;
                        if (!Enabled) break;
                        if (AskForDegree)
                        {
                            Il2CppSystem.Action<int> action = (Il2CppSystem.Action<int>)delegate (int x)
                            {
                                var y = Math.Min(100, x); var z = Math.Max(1, y);
                                info.totalInvestment = Game.instance.model.paragonDegreeDataModel.powerDegreeRequirements[z - 1];
                                tower.investmentInfo = info;
                                tower.UpdateDegree();
                            };
                            PopupScreen.instance.ShowSetValuePopup("Degree", "Set degree", action, 100);
                            break;
                        }
                        else
                        {
                            info.totalInvestment = Game.instance.model.paragonDegreeDataModel.powerDegreeRequirements[99];
                            tower.investmentInfo = info;
                            tower.UpdateDegree();
                        }
                        break;
                    }
                }
            }
        }
    }
}
