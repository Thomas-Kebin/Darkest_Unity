﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ReplacedQuirkIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform rectTransform;
    public QuirkSlot quirkSlot;
    public Image replacedIcon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(quirkSlot.QuirkInfo != null && quirkSlot.QuirkInfo.IsReplaced)
        {
            var tooltipFormat = LocalizationManager.GetString("str_quirk_replaces");
            tooltipFormat = string.Format(tooltipFormat, LocalizationManager.GetString(
                "str_quirk_name_" + quirkSlot.QuirkInfo.ReplacedQuirk));
            ToolTipManager.Instanse.Show(tooltipFormat, eventData, rectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
