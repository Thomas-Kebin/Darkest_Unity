﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CampingTrainerWindow : BuildingWindow
{
    public Text dragHeroLabel;
    public CampingTrainerHeroWindow heroOverview;
    public HeroObserverSlot heroSlot;

    public UpgradeButton upgradeSwitch;
    public UpgradeWindow upgradeWindow;

    public Button closeButton;

    public override TownManager TownManager { get; set; }
    public CampingTrainer CampingTrainer { get; private set; }

    void Awake()
    {
        heroSlot.OnHeroDropped += heroSlot_OnHeroDropped;
        heroSlot.OnHeroRemoved += heroSlot_OnHeroRemoved;
    }

    void heroSlot_OnHeroDropped(Hero hero)
    {
        heroOverview.gameObject.SetActive(true);
        heroOverview.LoadHeroOverview(hero);
        dragHeroLabel.enabled = false;
    }

    void heroSlot_OnHeroRemoved(Hero hero)
    {
        heroOverview.gameObject.SetActive(false);
        heroOverview.ResetWindow();
        dragHeroLabel.enabled = true;
    }

    void BlacksmithWindow_onUpgradeClick(BuildingUpgradeSlot slot)
    {
        var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(slot.Tree.Id, slot.UpgradeInfo);
        if (status == UpgradeStatus.Available)
        {
            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.UpgradeInfo))
            {
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                UpdateUpgradeTrees();
            }
        }
    }

    public override void Initialize()
    {
        heroOverview.Initialize(TownManager);

        CampingTrainer = DarkestDungeonManager.Campaign.Estate.CampingTrainer;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.CampingTrainer);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        foreach (var tree in upgradeWindow.upgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.upgrades.Count; i++)
            {
                tree.upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId];
                tree.upgrades[i].UpgradeInfo = currentUpgrades[i];
                tree.upgrades[i].TownUpgrades = new List<ITownUpgrade>(new ITownUpgrade[] {
                    CampingTrainer.GetUpgradeByCode(currentUpgrades[i].Code) });
                tree.upgrades[i].onClick += BlacksmithWindow_onUpgradeClick;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }
    }

    public void UpdateUpgradeTrees()
    {
        CampingTrainer.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.CampingTrainer);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        foreach (var tree in upgradeWindow.upgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.upgrades.Count; i++)
            {
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }
        heroOverview.UpdateHeroOverview();
    }

    public void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
        }
    }

    public override void WindowClosed()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
        heroSlot.ClearSlot();
        gameObject.SetActive(false);
        TownManager.BuildingWindowActive = false;
    }

    public void UpgradeSwitchClicked()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
        else
        {
            foreach (var tree in upgradeWindow.upgradeTrees)
            {
                for (int i = 0; i < tree.upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, tree.upgrades[i].UpgradeInfo);
                    if (status == UpgradeStatus.Available)
                        TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                }
            }
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(true);
        }
    }
}
