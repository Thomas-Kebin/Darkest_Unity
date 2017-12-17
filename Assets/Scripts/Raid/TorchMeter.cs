﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text;

public enum TorchRangeType
{
    Out, Dark, Dim, Shadowy, Radiant
}

public class TorchMeter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public class TorchRange
    {
        public TorchRangeType RangeType { get; set; }
        public string AnimationId { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        public float ScoutingChance { get; private set; }
        public float HeroesSurprised { get; private set; }
        public float MonstersSurprised { get; private set; }
        public List<Buff> HeroBuffs { get; private set; }
        public List<Buff> MonsterBuffs { get; private set; }

        public void SetChances(float scouting, float heroSuprised, float monsterSurprised)
        {
            ScoutingChance = scouting;
            HeroesSurprised = heroSuprised;
            MonstersSurprised = monsterSurprised;
        }

        public TorchRange(TorchRangeType torchType, string animationId, int min, int max)
        {
            RangeType = torchType;
            AnimationId = animationId;
            Min = min;
            Max = max;
            HeroBuffs = new List<Buff>();
            MonsterBuffs = new List<Buff>();
        }

        public bool InRange(int torchValue)
        {
            return Min <= torchValue && torchValue <= Max;
        }
    }

    public RectTransform leftRect;
    public RectTransform rightRect;
    public RectTransform torchRect;

    public SkeletonAnimation torchSwitch;
    public SkeletonAnimation torchFlame;
    public SkeletonAnimation torchSparks;

    public Animator torchAnimator;
    public CanvasGroup canvasGroup;

    public List<TorchRange> Ranges { get; private set; }
    public TorchRange CurrentRange { get; private set; }

    public int TorchAmount { get; private set; }
    public int MaxAmount { get; private set; }
    public TorchlightModifier Modifier { get; private set; }
    
    public bool IsActive { get; set; }

    private const int baseLength = 450;
    private int currentLength = baseLength;
    private int targetLength = baseLength;
    private int lastValue = 0;

    private float velocity = 0;
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
    float doubleTapTimer = 0f;
    float doubleTapTime = 0.2f;
#endif

    public void Show()
    {
        torchAnimator.SetBool("IsActive", true);
    }
    public void Hide()
    {
        torchAnimator.SetBool("IsActive", false);

        if(ToolTipManager.Instanse.CurrentTooltip != null)
        {
            if (ToolTipManager.Instanse.CurrentTooltip.SenderRect == torchRect)
                ToolTipManager.Instanse.Hide();
        }
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        if (doubleTapTimer > 0)
            doubleTapTimer -= Time.deltaTime;
#endif
        if(canvasGroup.alpha != torchSwitch.skeleton.A)
        {
            torchSwitch.skeleton.A = canvasGroup.alpha;
            torchSparks.skeleton.A = canvasGroup.alpha;
            torchFlame.skeleton.A = canvasGroup.alpha;
            RaidSceneManager.RaidEvents.RoundIndicator.indicator.skeleton.A = canvasGroup.alpha;
            RaidSceneManager.RaidEvents.RoundIndicator.canvasGroup.alpha = canvasGroup.alpha;
        }

        if(targetLength != currentLength)
        {
            currentLength = Mathf.RoundToInt(Mathf.SmoothDamp(currentLength, targetLength, ref velocity, 0.5f));
            leftRect.sizeDelta = new Vector2(currentLength, leftRect.sizeDelta.y);
            rightRect.sizeDelta = new Vector2(currentLength, rightRect.sizeDelta.y);
        }
    }
    void UpdateTorch()
    {
        RaidSceneManager.Inventory.UpdateState();
        targetLength = MaxAmount != 0 ? Mathf.RoundToInt((float)baseLength * TorchAmount / MaxAmount) : 0;

        for (int i = 0; i < Ranges.Count; i++)
        {
            if (Ranges[i].InRange(TorchAmount))
            {
                if (CurrentRange != Ranges[i])
                {
                    if(CurrentRange.Min < Ranges[i].Min)
                        torchSwitch.state.SetAnimation(0, "ignite", false);
                    else
                        torchSwitch.state.SetAnimation(0, "extinguish", false);

                    CurrentRange = Ranges[i];
                    if(CurrentRange.RangeType == TorchRangeType.Out)
                        DarkestSoundManager.ExecuteNarration("torchlight_out", NarrationPlace.Raid);
                    else if (CurrentRange.RangeType == TorchRangeType.Radiant && TorchAmount > 90)
                        DarkestSoundManager.ExecuteNarration("torchlight_full", NarrationPlace.Raid);

                    ApplyBuffs();
                    torchFlame.state.SetAnimation(0, CurrentRange.AnimationId, true);
                    break;
                }
                else
                    break;
            }
        }

        RaidSceneManager.Formations.heroes.UpdateBuffRule(BuffRule.LightAbove);
        RaidSceneManager.Formations.heroes.UpdateBuffRule(BuffRule.LightBelow);
    }
    void ApplyBuffs()
    {
        for (int j = 0; j < RaidSceneManager.Formations.heroes.party.Units.Count; j++)
        {
            RaidSceneManager.Formations.heroes.party.Units[j].Character.RemoveLightBuffs();
            for (int k = 0; k < CurrentRange.HeroBuffs.Count; k++)
                RaidSceneManager.Formations.heroes.party.Units[j].Character.AddBuff(
                    new BuffInfo(CurrentRange.HeroBuffs[k], BuffDurationType.Permanent, BuffSourceType.Light));

            if (RaidSceneManager.RaidPanel.SelectedUnit == RaidSceneManager.Formations.heroes.party.Units[j])
                RaidSceneManager.RaidPanel.heroPanel.UpdateHero();
        }

        if (RaidSceneManager.BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            for (int j = 0; j < RaidSceneManager.Formations.monsters.party.Units.Count; j++)
            {
                RaidSceneManager.Formations.monsters.party.Units[j].Character.RemoveLightBuffs();
                if (RaidSceneManager.Formations.monsters.party.Units[j].Character.IsMonster)
                {
                    for (int k = 0; k < CurrentRange.MonsterBuffs.Count; k++)
                        RaidSceneManager.Formations.monsters.party.Units[j].Character.AddBuff(
                            new BuffInfo(CurrentRange.MonsterBuffs[k], BuffDurationType.Permanent, BuffSourceType.Light));
                }
                else
                {
                    for (int k = 0; k < CurrentRange.HeroBuffs.Count; k++)
                        RaidSceneManager.Formations.monsters.party.Units[j].Character.AddBuff(
                            new BuffInfo(CurrentRange.HeroBuffs[k], BuffDurationType.Permanent, BuffSourceType.Light));
                }
            }
        }
    }

    public void Initialize(int torchValue, int maxAmount = 100)
    {
        torchSwitch.Reset();
        torchSparks.Reset();

        if (!torchSwitch.gameObject.activeSelf)
            torchSwitch.gameObject.SetActive(true);
        if (!torchSparks.gameObject.activeSelf)
            torchSparks.gameObject.SetActive(true);

        TorchAmount = Mathf.Clamp(torchValue, 0, maxAmount);
        MaxAmount = 100;

        Ranges = new List<TorchRange>()
        {
            new TorchRange(TorchRangeType.Radiant, "radiant_loop", 76, 100),
            new TorchRange(TorchRangeType.Dim, "dim_loop", 51, 75),
            new TorchRange(TorchRangeType.Shadowy, "shadowy_loop", 26, 50),
            new TorchRange(TorchRangeType.Dark, "dark_loop", 1, 25),
            new TorchRange(TorchRangeType.Out, "out_loop", 0, 0),
        };

        Ranges[0].SetChances(0.15f, 0, 0.25f);

        Ranges[1].SetChances(0.075f, 0, 0.15f);
        Ranges[1].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.StressDmgReceivedPercent,
            ModifierValue = 0.1f, Type = BuffType.StatAdd });
        Ranges[1].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.01f, Type = BuffType.StatAdd });

        Ranges[2].SetChances(0, 0.15f, 0.10f);
        Ranges[2].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.StressDmgReceivedPercent,
            ModifierValue = 0.2f, Type = BuffType.StatAdd });
        Ranges[2].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.CritChance,
            ModifierValue = 0.01f, Type = BuffType.StatAdd });
        Ranges[2].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.AttackRating,
            ModifierValue = 0.05f, Type = BuffType.StatAdd });
        Ranges[2].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageLow,
            ModifierValue = 0.1f, Type = BuffType.StatMultiply });
        Ranges[2].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.1f, Type = BuffType.StatMultiply });
        Ranges[2].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.02f, Type = BuffType.StatAdd });

        Ranges[3].SetChances(0, 0.25f, 0.05f);
        Ranges[3].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.StressDmgReceivedPercent,
            ModifierValue = 0.3f, Type = BuffType.StatAdd });
        Ranges[3].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.CritChance,
            ModifierValue = 0.02f, Type = BuffType.StatAdd });
        Ranges[3].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.AttackRating,
            ModifierValue = 0.1f, Type = BuffType.StatAdd });
        Ranges[3].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageLow,
            ModifierValue = 0.15f, Type = BuffType.StatMultiply });
        Ranges[3].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.15f, Type = BuffType.StatMultiply });
        Ranges[3].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.03f, Type = BuffType.StatAdd });

        Ranges[4].SetChances(0, 0.4f, 0);
        Ranges[4].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.StressDmgReceivedPercent,
            ModifierValue = 0.4f, Type = BuffType.StatAdd });
        Ranges[4].HeroBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.CritChance,
            ModifierValue = 0.03f, Type = BuffType.StatAdd });
        Ranges[4].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.AttackRating,
            ModifierValue = 0.125f, Type = BuffType.StatAdd });
        Ranges[4].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageLow,
            ModifierValue = 0.25f, Type = BuffType.StatMultiply });
        Ranges[4].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.25f, Type = BuffType.StatMultiply });
        Ranges[4].MonsterBuffs.Add(new Buff() { Id = "", AttributeType = AttributeType.DamageHigh,
            ModifierValue = 0.05f, Type = BuffType.StatAdd });

        for(int i = 0; i < Ranges.Count; i++)
        {
            if(Ranges[i].InRange(TorchAmount))
            {
                CurrentRange = Ranges[i];
                break;
            }
        }
        UpdateTorch();
    }

    public void ApplyBuffsForUnit(FormationUnit newUnit)
    {
        newUnit.Character.RemoveLightBuffs();
        if (newUnit.Character.IsMonster)
        {
            for (int k = 0; k < CurrentRange.MonsterBuffs.Count; k++)
                newUnit.Character.AddBuff(new BuffInfo(CurrentRange.MonsterBuffs[k], BuffDurationType.Permanent, BuffSourceType.Light));
        }
    }

    public void IncreaseTorch(int value)
    {
        if(Modifier != null)
            TorchAmount = Mathf.Clamp(TorchAmount + value, Modifier.Min, Modifier.Max);
        else
            TorchAmount = Mathf.Clamp(TorchAmount + value, 0, MaxAmount);

        torchSparks.state.SetAnimation(0, "sparks", false);

        UpdateTorch();
    }
    public void DecreaseTorch(int value)
    {
        if (Modifier != null)
            TorchAmount = Mathf.Clamp(TorchAmount - value, Modifier.Min, Modifier.Max);
        else
            TorchAmount = Mathf.Clamp(TorchAmount - value, 0, MaxAmount);

        if (!torchSwitch.gameObject.activeSelf)
            torchSwitch.gameObject.SetActive(true);
        
        UpdateTorch();
    }
    public void Modify(TorchlightModifier modifier)
    {
        Modifier = modifier;
        lastValue = TorchAmount;
        TorchAmount = Mathf.Clamp(TorchAmount, Modifier.Min, Modifier.Max);
        UpdateTorch();
    }
    public void ClearModifier()
    {
        if (Modifier != null)
        {
            Modifier = null;
            TorchAmount = lastValue;
            UpdateTorch();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!torchAnimator.GetBool("IsActive"))
            return;

        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
        sb.Append(LocalizationManager.GetString("str_darkness_title_" + Ranges.IndexOf(CurrentRange)) + "(" + TorchAmount + ")");
        sb.Append("</color>\n");
        switch(CurrentRange.RangeType)
        {
            case TorchRangeType.Radiant:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_player_scout"));
                sb.AppendLine();
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_monstersSurprised"));
                sb.AppendLine();
                sb.Append("</color>");
                break;
            case TorchRangeType.Dim:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append("+");
                sb.Append(LocalizationManager.GetString("str_darkness_stress"));
                sb.AppendLine();
                sb.Append("</color>");
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_player_scout"));
                sb.AppendLine();
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_monstersSurprised"));
                sb.AppendLine();
                sb.Append("</color>");
                break;
            case TorchRangeType.Shadowy:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_stress"));
                sb.AppendLine();
                sb.Append("+");
                sb.Append(LocalizationManager.GetString("str_darkness_monster"));
                sb.AppendLine();
                sb.Append("+");
                sb.Append(LocalizationManager.GetString("str_darkness_heroesSurprised"));
                sb.AppendLine();
                sb.Append("</color>");
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
                sb.Append("+");
                sb.Append(LocalizationManager.GetString("str_darkness_loot"));
                sb.AppendLine();
                sb.Append("+");
                sb.Append(LocalizationManager.GetString("str_darkness_player_crit"));
                sb.AppendLine();
                sb.Append("</color>");
                break;
            case TorchRangeType.Dark:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append("+++");
                sb.Append(LocalizationManager.GetString("str_darkness_stress"));
                sb.AppendLine();
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_monster"));
                sb.AppendLine();
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_heroesSurprised"));
                sb.AppendLine();
                sb.Append("</color>");
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_loot"));
                sb.AppendLine();
                sb.Append("++");
                sb.Append(LocalizationManager.GetString("str_darkness_player_crit"));
                sb.AppendLine();
                sb.Append("</color>");
                break;
            case TorchRangeType.Out:
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.Append("++++");
                sb.Append(LocalizationManager.GetString("str_darkness_stress"));
                sb.AppendLine();
                sb.Append("+++");
                sb.Append(LocalizationManager.GetString("str_darkness_monster"));
                sb.AppendLine();
                sb.Append("+++");
                sb.Append(LocalizationManager.GetString("str_darkness_heroesSurprised"));
                sb.AppendLine();
                sb.Append("</color>");
                sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
                sb.Append("+++");
                sb.Append(LocalizationManager.GetString("str_darkness_loot"));
                sb.AppendLine();
                sb.Append("+++");
                sb.Append(LocalizationManager.GetString("str_darkness_player_crit"));
                sb.AppendLine();
                sb.Append("</color>");
                break;

        }
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
        sb.Append(LocalizationManager.GetString("str_reduce_torch_tip"));
        sb.AppendLine();
        sb.Append(LocalizationManager.GetString("str_snuff_torch_tip"));
        sb.Append("</color>");
        ToolTipManager.Instanse.Show(sb.ToString(), eventData, torchRect, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!torchAnimator.GetBool("IsActive"))
            return;

        if (SceneManagerHelper.ActiveSceneName == "DungeonMultiplayer")
            return;

#if UNITY_ANDROID || UNITY_IOS
        if (doubleTapTimer > 0)
        {
            DecreaseTorch(10);

            if (ToolTipManager.Instanse.toolTip.isActiveAndEnabled)
                OnPointerEnter(eventData);
        }
        else
            doubleTapTimer = doubleTapTime;
#else
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.LeftControl))
                DecreaseTorch(100);
            else
                DecreaseTorch(10);

            if (ToolTipManager.Instanse.toolTip.isActiveAndEnabled)
                OnPointerEnter(eventData);
        }
#endif
    }
}