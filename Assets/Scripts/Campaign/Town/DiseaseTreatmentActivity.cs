﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class DiseaseTreatmentActivity
{
    public string Id { get; set; }

    #region Activity Parameters
    public float DiseaseTreatmentChance { get; set; }

    public CurrencyCost BaseDiseaseTreatmentCost { get; set; }
    public float BaseCureAllChance { get; set; }
    public int BaseDiseaseSlots { get; set; }

    public CurrencyCost DiseaseTreatmentCost { get; set; }
    public float CureAllChance { get; set; }
    public int DiseaseSlots { get; set; }
    #endregion

    #region Activity Upgrades
    public List<CostUpgrade> DiseaseTreatmentUpgrades { get; set; }
    public List<ChanceUpgrade> CureAllUpgrades { get; set; }
    public List<SlotUpgrade> SlotUpgrades { get; set; }
    #endregion

    public List<TreatmentSlot> TreatmentSlots { get; set; }

    void LogActivity(ActivityType type, Hero hero, string[] quirks)
    {
        var log = DarkestDungeonManager.Campaign.CurrentLog();
        if (log != null)
            log.HeroRecords.Add(new ActorActivityRecord(type, hero, quirks));
    }

    public DiseaseTreatmentActivity()
    {
        DiseaseTreatmentUpgrades = new List<CostUpgrade>();
        CureAllUpgrades = new List<ChanceUpgrade>();
        SlotUpgrades = new List<SlotUpgrade>();

        TreatmentSlots = new List<TreatmentSlot>();
    }

    public void Reset()
    {
        DiseaseTreatmentCost = BaseDiseaseTreatmentCost;
        CureAllChance = BaseCureAllChance;
        DiseaseSlots = BaseDiseaseSlots;

        TreatmentSlots = new List<TreatmentSlot>();
    }
    public void InitializeActivity(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();

        for (int i = DiseaseTreatmentUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[DiseaseTreatmentUpgrades[i].TreeId].PurchasedUpgrades.Contains(DiseaseTreatmentUpgrades[i].UpgradeCode))
            {
                DiseaseTreatmentCost = DiseaseTreatmentUpgrades[i].Cost;
                break;
            }
        }
        for (int i = CureAllUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[CureAllUpgrades[i].TreeId].PurchasedUpgrades.Contains(CureAllUpgrades[i].UpgradeCode))
            {
                CureAllChance = CureAllUpgrades[i].Chance;
                break;
            }
        }

        for (int i = SlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[SlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(SlotUpgrades[i].UpgradeCode))
            {
                DiseaseSlots = SlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        for (int i = 1; i <= 3; i++)
        {
            if (i <= DiseaseSlots)
                TreatmentSlots.Add(new TreatmentSlot(true, 0, 0, 0, DiseaseTreatmentCost.Amount));
            else
                TreatmentSlots.Add(new TreatmentSlot(false, 0, 0, 0, DiseaseTreatmentCost.Amount));
        }
    }
    public void ProvideActivity()
    {
        foreach (var treatmentSlot in TreatmentSlots)
        {
            switch (treatmentSlot.Status)
            {
                case ActivitySlotStatus.Caretaken:
                    treatmentSlot.Status = ActivitySlotStatus.Available;
                    treatmentSlot.Hero = null;
                    break;
                case ActivitySlotStatus.Blocked:
                case ActivitySlotStatus.Checkout:
                    treatmentSlot.Hero.Status = HeroStatus.Available;
                    treatmentSlot.Status = ActivitySlotStatus.Available;
                    treatmentSlot.Hero = null;
                    break;
                case ActivitySlotStatus.Paid:
                    if (treatmentSlot.TargetDiseaseQuirk != null)
                    {
                        if(RandomSolver.CheckSuccess(CureAllChance))
                        {
                            var cured = treatmentSlot.Hero.RemoveDiseases();
                            if (cured.Count == 0)
                                Debug.LogError("Sanitarium treament diseases not found.");
                            else
                                LogActivity(ActivityType.RemoveAllDiseases, treatmentSlot.Hero, cured.Select(info => info.Quirk.Id).ToArray());
                        }
                        else
                        {
                            if (treatmentSlot.Hero.RemoveQuirk(treatmentSlot.TargetDiseaseQuirk) == null)
                                Debug.LogError("Sanitarium treament disease not found.");
                            else
                                LogActivity(ActivityType.RemoveDisease, treatmentSlot.Hero, new string[] {treatmentSlot.TargetDiseaseQuirk});
                        }
                    }
                    treatmentSlot.Hero.Status = HeroStatus.Available;
                    treatmentSlot.Status = ActivitySlotStatus.Available;
                    treatmentSlot.Hero = null;
                    break;
                default:
                    break;
            }
        }
    }
    public void UpdateActivity(Dictionary<string, UpgradePurchases> purchases)
    {
        for (int i = DiseaseTreatmentUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[DiseaseTreatmentUpgrades[i].TreeId].PurchasedUpgrades.Contains(DiseaseTreatmentUpgrades[i].UpgradeCode))
            {
                DiseaseTreatmentCost = DiseaseTreatmentUpgrades[i].Cost;
                break;
            }
        }
        for (int i = CureAllUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[CureAllUpgrades[i].TreeId].PurchasedUpgrades.Contains(CureAllUpgrades[i].UpgradeCode))
            {
                CureAllChance = CureAllUpgrades[i].Chance;
                break;
            }
        }

        for (int i = SlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[SlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(SlotUpgrades[i].UpgradeCode))
            {
                DiseaseSlots = SlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        for (int i = 1; i <= 3; i++)
        {
            if (i <= DiseaseSlots)
                TreatmentSlots[i - 1].UpdateTreatmentSlot(true, 0, 0, 0, DiseaseTreatmentCost.Amount);
            else
                TreatmentSlots[i - 1].UpdateTreatmentSlot(false, 0, 0, 0, DiseaseTreatmentCost.Amount);
        }
    }
    public void UpdateActivitySlots(SaveCampaignData saveData)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i + 1 <= DiseaseSlots)
            {
                if (saveData.sanitariumActivitySlots.Count > 1 && saveData.sanitariumActivitySlots[1].Count >= i)
                {
                    var activityHero = DarkestDungeonManager.Campaign.Heroes.Find(hero =>
                        hero.RosterId == saveData.sanitariumActivitySlots[1][i].HeroRosterId);

                    TreatmentSlots[i].Hero = activityHero;
                    if (activityHero != null)
                        TreatmentSlots[i].Status = saveData.sanitariumActivitySlots[1][i].Status;
                    else
                        TreatmentSlots[i].Status = ActivitySlotStatus.Available;

                    TreatmentSlots[i].TargetDiseaseQuirk = saveData.sanitariumActivitySlots[1][i].TargetDiseaseQuirk;
                    TreatmentSlots[i].TargetPositiveQuirk = saveData.sanitariumActivitySlots[1][i].TargetPositiveQuirk;
                    TreatmentSlots[i].TargetNegativeQuirk = saveData.sanitariumActivitySlots[1][i].TargetNegativeQuirk;

                    TreatmentSlots[i].UpdateTreatmentSlot(true, 0, 0, 0, DiseaseTreatmentCost.Amount);
                }
                else
                    TreatmentSlots[i].UpdateTreatmentSlot(true, 0, 0, 0, DiseaseTreatmentCost.Amount);

            }
            else
                TreatmentSlots[i].UpdateTreatmentSlot(false, 0, 0, 0, DiseaseTreatmentCost.Amount);
        }
    }
    public List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        List<ITownUpgrade> foundUpgrades = new List<ITownUpgrade>();
        ITownUpgrade upgrade = DiseaseTreatmentUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        upgrade = CureAllUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        upgrade = SlotUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        return foundUpgrades;
    }
}