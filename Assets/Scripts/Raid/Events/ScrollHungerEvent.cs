﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum HungerResultType { Wait, Eat, Starve }

public class ScrollHungerEvent : MonoBehaviour
{
    public Text title;
    public Text description;

    public QuickParameterTip eatTip;
    public QuickParameterTip starveTip;
    public Button eatButton;

    public HungerResultType ActionType { get; private set; }
    public int MealAmount { get; private set; }

    public void Show()
    {
    }
    public void Hide()
    {
    }

    public void EatSelected()
    {
        if (ActionType == HungerResultType.Wait)
        {
            ActionType = HungerResultType.Eat;
        }
    }
    public void StarveSelected()
    {
        if (ActionType == HungerResultType.Wait)
        {
            ActionType = HungerResultType.Starve;
        }
    }
    public void LoadHungerEventMeal()
    {
        ActionType = HungerResultType.Wait;

        int baseMeal = 0;
        for (int i = 0; i < RaidSceneManager.HeroParty.Units.Count; i++)
            baseMeal += Mathf.RoundToInt(1 + RaidSceneManager.HeroParty.Units[i].Character.FoodConsumption);

        MealAmount = Mathf.Clamp(baseMeal, 1, 12);
        if (RaidSceneManager.Inventory.ContainsEnoughItems("provision", MealAmount))
            eatButton.interactable = true;
        else
            eatButton.interactable = false;

        eatTip.ParamCount = 2;
        eatTip.ParamOne = MealAmount;
        eatTip.ParamTwo = 0.05f;

        starveTip.ParamCount = 1;
        starveTip.ParamOne = 0.2f;

        ScrollOpened();
    }
    public void ScrollOpened()
    {
        gameObject.SetActive(true);
    }
    public void ScrollClosed()
    {
        gameObject.SetActive(false);
        ToolTipManager.Instanse.Hide();
    }
}
