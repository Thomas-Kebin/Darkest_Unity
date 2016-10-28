﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
    IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public Image itemIcon;
    public Image rarityIcon;
    public Text amountText;

    public string ItemType
    {
        get
        {
            return Item.Type;
        }
    }
    public int Amount
    {
        get
        {
            return Item.Amount;
        }
    }
    public int GoldPrice
    {
        get
        {
            return Item.Amount * ItemData.PurchasePrice;
        }
    }

    public bool IsNotEmpty
    {
        get
        { 
            return !isEmpty;
        }
    }
    public bool IsFull
    {
        get
        {
            if (Amount >= ItemData.StackLimit)
                return true;

            return false;
        }
    }
    public bool Deactivated
    {
        get;
        set;
    }
    public bool Highlighted
    {
        get;
        set;
    }

    public bool IsSameItem(ItemDefinition compareItem)
    {
        if (isEmpty)
            return false;

        return Item.IsSameItem(compareItem);
    }
    public bool HasFreeSpaceForItem(ItemDefinition compareItem)
    {
        if (isEmpty)
            return false;

        if (Item.IsSameItem(compareItem))
            return Item.Amount < ItemData.StackLimit;

        return false;
    }

    public ItemDefinition Item { get; set; }
    public ItemData ItemData { get; set; }
    public InventorySlot Slot { get; set; }
    public RectTransform RectTransform { get; set; }

    bool isDragged;
    bool isEmpty;
    bool isUnavailable;

    void LoadItem()
    {
        ItemData = DarkestDungeonManager.Data.Items[Item.Type][Item.Id];
        if(Item.Type == "trinket")
        {
            LoadTrinket(ItemData as Trinket);
            return;
        }
        else if (Item.Type == "journal_page")
        {
            itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_journal_page"];
        }
        else if (Item.Type == "gold" || Item.Type == "provision")
        {
            int thresholdIndex = Mathf.Clamp(Mathf.RoundToInt((float)Item.Amount / ItemData.StackLimit / 0.25f), 0, 3);
            itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id + "_" + thresholdIndex];
        }
        else
            itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id];

        UpdateAmount();

        gameObject.SetActive(true);
    }
    void LoadTrinket(Trinket trinket)
    {
        ItemData = trinket;

        itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id];
        rarityIcon.sprite = DarkestDungeonManager.Data.Sprites["rarity_" + trinket.RarityId];

        UpdateAmount();

        gameObject.SetActive(true);
    }
    void UpdateAmount()
    {
        if (Item.Amount > 1)
            amountText.text = Amount.ToString();
        else
            amountText.text = "";

        if (Item.Type == "gold" || Item.Type == "provision")
        {
            int thresholdIndex = Mathf.Clamp(Mathf.RoundToInt((float)Item.Amount / ItemData.StackLimit / 0.2f) - 1, 0, 3);
            itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id + "_" + thresholdIndex];
        }
    }

    public void Initialize(InventorySlot inventorySlot)
    {
        isEmpty = true;
        isDragged = false;

        Item = new ItemDefinition("", "", 0);
        Slot = inventorySlot;
        RectTransform = GetComponent<RectTransform>();
        ItemData = null;

        gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        Deactivated = !active;

        if(active)
        {
            if(Highlighted)
            {
                itemIcon.material = DarkestDungeonManager.HighlightMaterial;
                amountText.material = DarkestDungeonManager.HighlightMaterial;
                rarityIcon.material = DarkestDungeonManager.HighlightMaterial;
            }
            else
            {
                itemIcon.material = itemIcon.defaultMaterial;
                amountText.material = amountText.defaultMaterial;
                rarityIcon.material = rarityIcon.defaultMaterial;
            }
        }
        else
        {
            if (Highlighted)
            {
                itemIcon.material = DarkestDungeonManager.DeactivatedHighlightedMaterial;
                amountText.material = DarkestDungeonManager.DeactivatedHighlightedMaterial;
                rarityIcon.material = DarkestDungeonManager.DeactivatedHighlightedMaterial;
            }
            else
            {
                itemIcon.material = DarkestDungeonManager.DeactivatedMaterial;
                amountText.material = DarkestDungeonManager.DeactivatedMaterial;
                rarityIcon.material = DarkestDungeonManager.DeactivatedMaterial;
            }
        }
    }
    public void SetPeacefulState(bool looting)
    {
        if (RaidSceneManager.Instanse == null)
            return;

        if(IsNotEmpty)
        {
            switch (ItemData.Type)
            {
                case "heirloom":
                case "gold":
                case "gem":
                case "journal_page":
                case "quest_item":
                    Deactivated = false;
                    break;
                case "supply":
                    switch(ItemData.Id)
                    {
                        case "shovel":
                            Deactivated = true;
                            break;
                        case "firewood":
                            if(RaidSceneManager.SceneState == DungeonSceneState.Room && looting == false)
                                Deactivated = false;
                            else
                                Deactivated = true;
                            break;
                        case "bandage":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null &&
                                RaidSceneManager.RaidPanel.SelectedHero[StatusType.Bleeding].IsApplied)
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "antivenom":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null && 
                                RaidSceneManager.RaidPanel.SelectedHero[StatusType.Poison].IsApplied)
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "skeleton_key":
                            Deactivated = true;
                            break;
                        case "medicinal_herbs":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null &&
                                RaidSceneManager.RaidPanel.SelectedHero.HasDebuffs())
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "torch":
                            if(RaidSceneManager.TorchMeter.TorchAmount < RaidSceneManager.TorchMeter.MaxAmount)
                                Deactivated = false;
                            else
                                Deactivated = true;
                            break;
                        case "holy_water":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null)
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "dog_treats":
                            Deactivated = true;
                            break;
                        default:
                            Deactivated = true;
                            break;
                    }
                    break;
                case "provision":
                    if (RaidSceneManager.RaidPanel.SelectedHero != null && 
                        RaidSceneManager.RaidPanel.SelectedHero.Health.ValueRatio < 1)
                        Deactivated = false;
                    else
                        Deactivated = true;
                    break;
                default:
                    Deactivated = true;
                    break;
            }
        }
        else
            Deactivated = true;

        SetActive(!Deactivated);
    }
    public void SetCombatState()
    {
        if (RaidSceneManager.Instanse == null)
            return;

        if (IsNotEmpty)
        {
            switch (ItemData.Type)
            {
                case "heirloom":
                case "gold":
                case "gem":
                case "journal_page":
                case "quest_item":
                    Deactivated = true;
                    break;
                case "supply":
                    switch (ItemData.Id)
                    {
                        case "shovel":
                            Deactivated = true;
                            break;
                        case "firewood":
                            Deactivated = true;
                            break;
                        case "bandage":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null &&
                                RaidSceneManager.RaidPanel.SelectedHero[StatusType.Bleeding].IsApplied)
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "antivenom":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null &&
                                RaidSceneManager.RaidPanel.SelectedHero[StatusType.Poison].IsApplied)
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "skeleton_key":
                            Deactivated = true;
                            break;
                        case "medicinal_herbs":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null && RaidSceneManager.RaidPanel.SelectedHero.HasDebuffs())
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "torch":
                            if (RaidSceneManager.TorchMeter.TorchAmount < RaidSceneManager.TorchMeter.MaxAmount)
                                Deactivated = false;
                            else
                                Deactivated = true;
                            break;
                        case "holy_water":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null)
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        case "dog_treats":
                            if (RaidSceneManager.RaidPanel.SelectedHero != null &&
                                RaidSceneManager.RaidPanel.SelectedHero.Class == "hound_master")
                            {
                                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.BlockedItems.Contains(ItemData.Id))
                                    Deactivated = true;
                                else
                                    Deactivated = false;
                            }
                            else
                                Deactivated = true;
                            break;
                        default:
                            Deactivated = true;
                            break;
                    }
                    break;
                case "provision":
                    Deactivated = true;
                    break;
                default:
                    Deactivated = true;
                    break;
            }
        }
        else
            Deactivated = true;

        SetActive(!Deactivated);
    }
    public void SetObstacleState()
    {
        if (RaidSceneManager.Instanse == null)
            return;

        if (IsNotEmpty)
        {
            switch (ItemData.Type)
            {
                case "supply":
                    if(ItemData.Id == "shovel")
                        Deactivated = false;
                    else
                        Deactivated = true;
                    break;
                default:
                    Deactivated = true;
                    break;
            }
        }
        else
            Deactivated = true;

        SetActive(!Deactivated);
    }
    public void SetInteractionState(bool questInteraction)
    {
        if (RaidSceneManager.Instanse == null)
            return;

        if (IsNotEmpty)
        {
            if(questInteraction)
            {
                switch (ItemData.Type)
                {
                    case "quest_item":
                        Deactivated = false;
                        break;
                    default:
                        Deactivated = true;
                        break;
                }
            }
            else
            {
                switch (ItemData.Type)
                {
                    case "supply":
                        Deactivated = false;
                        break;
                    default:
                        Deactivated = true;
                        break;
                }
            }
        }
        else
            Deactivated = true;

        SetActive(!Deactivated);
    }

    public void SetDropUnavailable()
    {
        if (Slot.overlayIcon != null)
        {
            isUnavailable = true;
            Slot.overlayIcon.enabled = true;
            Slot.overlayIcon.sprite = DarkestDungeonManager.Data.Sprites["eqp_unavailable_mouseover"];
        }
    }
    public void SetOverlayDefault()
    {
        if (Slot.overlayIcon != null)
        {
            isUnavailable = false;
            Slot.overlayIcon.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");

        SetActive(!Deactivated);

        if(Slot.Inventory != null && Slot.Inventory.Configuration == InventoryConfiguration.RaidInventory && Item.Type != "quest_item"
            && (Slot.Inventory.State == InventoryState.Peaceful || Slot.Inventory.State == InventoryState.PeacefulLooting))
            ToolTipManager.Instanse.Show(Item.ToolTipDiscard, eventData, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        else
            ToolTipManager.Instanse.Show(Item.ToolTip, eventData, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (Slot.overlayIcon != null && !isUnavailable)
            Slot.overlayIcon.enabled = false;
        SetActive(!Deactivated);
        ToolTipManager.Instanse.Hide();
    }

    public void Create(string itemType, string itemId, int amount)
    {
        isEmpty = false;
        Item.Type = itemType;
        Item.Id = itemId;
        Item.Amount = amount;
        LoadItem();
    }
    public void Create(ItemDefinition item)
    {
        isEmpty = false;
        Item.Type = item.Type;
        Item.Id = item.Id;
        Item.Amount = item.Amount;
        LoadItem();
    }
    public void Create(Trinket trinket)
    {
        isEmpty = false;
        Item.Type = trinket.Type;
        Item.Id = trinket.Id;
        Item.Amount = 1;
        LoadTrinket(trinket);
    }
    public void Create(InventorySlotData slotData)
    {
        isEmpty = slotData.ItemData == null;
        if (isEmpty)
            Delete();
        else
        {
            Item.Type = slotData.Item.Type;
            Item.Id = slotData.Item.Id;
            Item.Amount = slotData.Item.Amount;
            LoadItem();
        }
    }
    public void Delete()
    {
        if (isDragged)
            OnEndDrag(null);

        isEmpty = true;

        Item.Type = "";
        Item.Id = "";
        Item.Amount = 0;

        ItemData = null;
        
        gameObject.SetActive(false);
    }

    public int AddItems(int addAmount)
    {
        if (addAmount == 0)
            return 0;
        if (Amount == ItemData.StackLimit)
            return addAmount;

        int itemsLeft = addAmount;
        if (Item.Amount + addAmount > ItemData.StackLimit)
        {
            itemsLeft = addAmount - (ItemData.StackLimit - Item.Amount);
            Item.Amount = ItemData.StackLimit;
        }
        else
        {
            itemsLeft = 0;
            Item.Amount += addAmount;
        }

        UpdateAmount();

        return itemsLeft;
    }
    public void RemoveItems(int remAmount)
    {
        if (remAmount > Item.Amount)
            Item.Amount = 0;
        else
            Item.Amount -= remAmount;

        if (Item.Amount == 0)
            Delete();
        else
            UpdateAmount();
    }
    public void MergeItems(InventoryItem itemSource)
    {
        int stackLimit = ItemData.StackLimit;

        int transferAmount = Mathf.Min(stackLimit - Amount, itemSource.Item.Amount);

        Item.Amount += transferAmount;
        UpdateAmount();

        itemSource.Item.Amount -= transferAmount;
        if (itemSource.Item.Amount == 0)
            itemSource.Delete();
        else
            itemSource.UpdateAmount();

    }

    public virtual void CopyToDragItem(DragItemHolder dragItem)
    {
        if (Item.Type == "trinket")
        {
            dragItem.backIcon.enabled = true;
            dragItem.backIcon.sprite = rarityIcon.sprite;
            dragItem.itemIcon.sprite = itemIcon.sprite;
        }
        else
        {
            dragItem.backIcon.enabled = false;
            dragItem.itemIcon.sprite = itemIcon.sprite;
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!isDragged)
            return;
        DragManager.Instanse.OnDrag(this, eventData);
    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if ((Slot.Inventory.Configuration == InventoryConfiguration.Equipment &&
            Slot.Inventory.State == InventoryState.Disabled) || eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.dragging = false;
            eventData.pointerDrag = null;
            return;
        }
        if (Slot.Inventory.Configuration == InventoryConfiguration.LootInventory &&
            Slot.Inventory.State == InventoryState.Disabled)
        {
            eventData.dragging = false;
            eventData.pointerDrag = null;
            return;
        }

        isDragged = true;
        DragManager.Instanse.StartDragging(this, eventData);
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragged)
            return;

        DragManager.Instanse.EndDragging(this, eventData);
        isDragged = false;
    }
    public virtual void OnDrop(PointerEventData eventData)
    {
        if (isDragged)
            return;

        Slot.OnDrop(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || isEmpty || isDragged)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(Input.GetKey(KeyCode.LeftShift) && 
                (Slot.Inventory.State == InventoryState.Peaceful || Slot.Inventory.State == InventoryState.PeacefulLooting))
            {
                if(Slot.Inventory.Configuration == InventoryConfiguration.RaidInventory && Item.Type != "quest_item")
                    RemoveItems(1);
                return;
            }
            Slot.SlotActivated();
            return;
        }

        if (!RaidSceneManager.Instanse || Deactivated)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            RaidSceneManager.Instanse.HeroItemActivated(Slot);
    }
}