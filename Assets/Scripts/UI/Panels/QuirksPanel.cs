﻿using UnityEngine;
using System.Collections.Generic;

public class QuirksPanel : MonoBehaviour
{
    public RectTransform positiveQuirksRect;
    public RectTransform negativeQuirksRect;

    public List<QuirkSlot> PositiveQuirkSlots { get; set; }
    public List<QuirkSlot> NegativeQuirkSlots { get; set; }

    void Awake()
    {
        PositiveQuirkSlots = new List<QuirkSlot>(positiveQuirksRect.GetComponentsInChildren<QuirkSlot>());
        NegativeQuirkSlots = new List<QuirkSlot>(negativeQuirksRect.GetComponentsInChildren<QuirkSlot>());
    }

    public void UpdateQuirksPanel(Hero hero)
    {
        int positiveQuirks = 0;
        foreach(var posQuirk in hero.PositiveQuirks)
            PositiveQuirkSlots[positiveQuirks++].UpdateQuirk(posQuirk);

        for (int i = positiveQuirks; i < PositiveQuirkSlots.Count; i++)
            PositiveQuirkSlots[i].UpdateQuirk(null);

        int negQuirks = 0;
        foreach (var negQuirk in hero.NegativeQuirks)
            NegativeQuirkSlots[negQuirks++].UpdateQuirk(negQuirk);

        for (int i = negQuirks; i < NegativeQuirkSlots.Count; i++)
            NegativeQuirkSlots[i].UpdateQuirk(null);
    }
}
