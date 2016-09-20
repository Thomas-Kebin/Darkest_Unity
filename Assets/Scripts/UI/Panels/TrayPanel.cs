﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TrayPanel : MonoBehaviour
{
    public RectTransform rectTransform;
    public GridLayoutGroup traySlotGrid;
    public List<TraySlot> traySlots;

    public FormationUnit TargetUnit { get; set; }

    void Awake()
    {
        for (int i = 0; i < traySlots.Count; i++)
            traySlots[i].TrayPanel = this;
    }

    public void UpdatePanel(FormationUnit target)
    {
        traySlotGrid.constraintCount = 4 * target.Size;
        rectTransform.sizeDelta = new Vector2(100 * target.Size, 50);
        TargetUnit = target;
        foreach (var traySlot in traySlots)
            traySlot.UpdateTraySlot();
    }
    public void ResetPanel()
    {
        TargetUnit = null;
    }

}
