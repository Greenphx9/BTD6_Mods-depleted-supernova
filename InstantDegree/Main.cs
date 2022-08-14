using Assets.Scripts;
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
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(InstantDegree.Main), "Instant Degrees", "2.2", "DepletedNova")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace InstantDegree
{
    public class Main : BloonsTD6Mod
    {
        public static ModSettingBool enabled = new ModSettingBool(true) { displayName = "Enabled" };
        public static ModSettingBool degree = new ModSettingBool(false) { displayName = "Set degree on creation" };

        // "askDegree" on upgrade
        [HarmonyPatch(typeof(UnityToSimulation), nameof(UnityToSimulation.UpgradeTowerParagon))]
        class UpgradeToParagon
        {
            [HarmonyPostfix]
            internal static void Postfix(ObjectId id)
            {
                if (degree && enabled)
                {
                    foreach (var simTower in InGame.instance.UnityToSimulation.GetAllTowers())
                    {
                        if (simTower.id == id)
                        {
                            var tower = simTower.tower.GetTowerBehavior<ParagonTower>();
                            Il2CppSystem.Action<int> action = (Il2CppSystem.Action<int>)delegate (int x)
                            {
                                setDegree(x, ref tower);
                                if (tower.isActive) tower.CreateDegreeText();
                            };
                            PopupScreen.instance.ShowSetValuePopup("Instant Degree", "Set Paragon degree:", action, 100);
                            break;
                        }
                    }
                }
            }
        }

        // UI
        static GameObject ParagonButton = null;

        [HarmonyPatch(typeof(TSMThemeDefault), nameof(TSMThemeDefault.TowerInfoChanged))]
        internal class TSMThemeDefault_TowerInfoChanged
        {
            public static TowerToSimulation selectedTower = null;

            [HarmonyPostfix]
            public static void Postfix(TSMThemeDefault __instance, TowerToSimulation tower)
            {
                if (tower.IsParagon)
                {
                    selectedTower = tower;
                    if (ParagonButton == null && enabled)
                    {
                        var Container = __instance.transform.parent.parent.parent.FindChild("SelectedTowerOptions").FindChild("ParagonDetails").FindChild("ParagonInfo");
                        var Obj = __instance.transform.FindChild("CloseButton").gameObject;
                        ParagonButton = Object.Instantiate(Obj, Container, true);
                        var Position = ParagonButton.transform.localPosition = new Vector3(0, 0, 0);
                        var rect = ParagonButton.GetComponent<RectTransform>();
                        rect.localPosition = new Vector3(-140, 10);
                        rect.sizeDelta = new Vector2(643, 235);
                        ParagonButton.GetComponent<Image>().SetSprite(ModContent.CreateSpriteReference(ModContent.GetTextureGUID<Main>("DegreeButton")));
                        ParagonButton.gameObject.GetComponent<Button>().SetOnClick(() =>
                        {
                            var paragonTower = selectedTower.tower.GetTowerBehavior<ParagonTower>();
                            Il2CppSystem.Action<int> action = (Il2CppSystem.Action<int>)delegate (int x)
                            {
                                setDegree(x, ref paragonTower);
                                paragonTower.CreateDegreeText();
                            };
                            PopupScreen.instance.ShowSetValuePopup("Instant Degree", "Set Paragon degree:", action, 100);
                        });
                    }
                    else if (ParagonButton != null && !enabled )
                    {
                        ParagonButton.Destroy();
                        ParagonButton = null;
                    }
                }
            }
        }

        // Degree util
        public static void setDegree(int x, ref ParagonTower paragonTower)
        {
            var y = Math.Min(100, x); var z = Math.Max(1, y);
            ParagonTower.InvestmentInfo info = paragonTower.investmentInfo;
            info.totalInvestment = Game.instance.model.paragonDegreeDataModel.powerDegreeRequirements[z - 1];
            paragonTower.investmentInfo = info;
            paragonTower.UpdateDegree();
        }
    }
}
