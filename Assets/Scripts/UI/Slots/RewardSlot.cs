﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RewardSlot : BaseSlot, IPointerEnterHandler, IPointerExitHandler
{
    public Image rarityFrame;
    public Image itemFrame;
    public Text amountText;

    public ItemDefinition Item { get; set; }

    public void SetItem(ItemDefinition item)
    {
        Item = item;
        if (Item.Type == "journal_page")
        {
            rarityFrame.enabled = false;
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_journal_page"];
            amountText.text = "";
        }
        else if (Item.Type == "gold" || Item.Type == "provision")
        {
            rarityFrame.enabled = false;
            int thresholdIndex = Mathf.Clamp(Mathf.RoundToInt((float)Item.Amount /
                DarkestDungeonManager.Data.Items[Item.Type][Item.Id].StackLimit / 0.25f), 0, 3);
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id + "_" + thresholdIndex];
            amountText.text = Item.Amount.ToString();
        }
        else if (item.Type == "trinket")
        {
            rarityFrame.enabled = true;
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_" + item.Type + "+" + item.Id];
            Trinket trinket = DarkestDungeonManager.Data.Items[item.Type][item.Id] as Trinket;
            rarityFrame.sprite = DarkestDungeonManager.Data.Sprites["rarity_" + trinket.Rarity];
            if (item.Amount > 1)
                amountText.text = Item.Amount.ToString();
            else
                amountText.text = "";
        }
        else
        {
            rarityFrame.enabled = false;
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_" + item.Type + "+" + item.Id];
            amountText.text = Item.Amount.ToString();
        }
        
    }
    public void SetSingle(ItemDefinition item)
    {
        Item = item;
        if (Item.Type == "journal_page")
        {
            rarityFrame.enabled = false;
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_journal_page"];
        }
        else if (Item.Type == "gold" || Item.Type == "provision")
        {
            rarityFrame.enabled = false;
            int thresholdIndex = Mathf.Clamp(Mathf.RoundToInt((float)Item.Amount /
                DarkestDungeonManager.Data.Items[Item.Type][Item.Id].StackLimit / 0.25f), 0, 3);
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id + "_" + thresholdIndex];
        }
        else if (item.Type == "trinket")
        {
            rarityFrame.enabled = true;
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_" + item.Type + "+" + item.Id];
            Trinket trinket = DarkestDungeonManager.Data.Items[item.Type][item.Id] as Trinket;
            rarityFrame.sprite = DarkestDungeonManager.Data.Sprites["rarity_" + trinket.Rarity];
        }
        else
        {
            rarityFrame.enabled = false;
            itemFrame.sprite = DarkestDungeonManager.Data.Sprites["inv_" + item.Type + "+" + item.Id];
        }
        amountText.text = "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item != null)
            ToolTipManager.Instanse.Show(Item.ToolTip, eventData, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}