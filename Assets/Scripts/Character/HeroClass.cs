﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class HeroClass
{
    public int IndexId { get; set; }
    public int TargetRank { get; set; }
    public string StringId { get; set; }
    public int RenderingRankOverride { get; set; }
    public CommonEffects CommonEffects { get; set; }
    public List<SkillArtInfo> SkillArtInfo { get; set; }

    public Dictionary<AttributeType, float> Resistanses { get; set; }
    public List<string> Tags { get; set; }

    public List<Equipment> Weapons { get; set; }
    public List<Equipment> Armors { get; set; }

    public List<CombatSkill> CombatSkills { get; private set; }
    public List<CombatSkill> CombatSkillVariants { get; private set; }
    public List<CampingSkill> CampingSkills { get; private set; }

    public MoveSkill MoveSkill { get; set; }
    public CombatSkill RiposteSkill { get; set; }
    public DeathDoor DeathDoor { get; set; }
    public HeroGeneration Generation { get; set; }
    public LootDefinition ExtraBattleLoot { get; set; }
    public LootDefinition ExtraCurioLoot { get; set; }
    public List<CharacterMode> Modes { get; set; }

    public string ExtraStackLimit { get; set; }
    public string IncompatiablePartyTag { get; set; }
    public bool CanSelectCombatSkills { get; set; }
    public int NumberOfSelectedCombatSkills { get; set; }

    public Dictionary<CharacterComponentType, CharacterComponent> Components { get; private set; }

    public HeroClass(List<string> data)
    {
        Components = new Dictionary<CharacterComponentType, CharacterComponent>();
        Resistanses = new Dictionary<AttributeType, float>();
        Tags = new List<string>();

        Weapons = new List<Equipment>();
        Armors = new List<Equipment>();

        CombatSkillVariants = new List<CombatSkill>();
        SkillArtInfo = new List<SkillArtInfo>();
        Modes = new List<CharacterMode>();

        LoadData(data);

        CampingSkills = DarkestDungeonManager.Data.CampingSkills.FindAll(skill => skill.Classes.Contains(StringId));
    }

    public void LoadData(List<string> heroData)
    {
        int index = 2;
        StringId = heroData[0].Split(' ')[1];

        while(heroData[index] != ".end")
        {
            List<string> data = heroData[index++].Replace("%", "").Replace("\"", "").
                Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            switch(data[0])
            {
                case "rendering:":
                    RenderingRankOverride = int.Parse(data[2]);
                    break;
                case "commonfx:":
                    if (CommonEffects == null)
                        CommonEffects = new CommonEffects(data);
                    else
                        CommonEffects.LoadData(data);
                    break;
                case "combat_skill:":
                    SkillArtInfo skillArt = new SkillArtInfo(data, false);
                    SkillArtInfo.Add(skillArt);
                    break;
                case "riposte_skill:":
                    SkillArtInfo riposteArt = new SkillArtInfo(data, false);
                    SkillArtInfo.Add(riposteArt);
                    break;
                default:
                    Debug.LogError("Unknown art token in hero: " + StringId);
                    break;
            }
        }
        index += 2;

        while (heroData[index] != ".end")
        {
            List<string> data = heroData[index++].Replace("%", "").Replace("\"", "").
                Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            switch (data[0])
            {
                case "resistances:":
                    Resistanses.Add(AttributeType.Stun, float.Parse(data[2]) / 100);
                    Resistanses.Add(AttributeType.Poison, float.Parse(data[4]) / 100);
                    Resistanses.Add(AttributeType.Bleed, float.Parse(data[6]) / 100);
                    Resistanses.Add(AttributeType.Disease, float.Parse(data[8]) / 100);
                    Resistanses.Add(AttributeType.Move, float.Parse(data[10]) / 100);
                    Resistanses.Add(AttributeType.Debuff, float.Parse(data[12]) / 100);
                    Resistanses.Add(AttributeType.DeathBlow, float.Parse(data[14]) / 100);
                    Resistanses.Add(AttributeType.Trap, float.Parse(data[16]) / 100);
                    break;
                case "weapon:":
                    Equipment weapon = new Equipment(data[2], Weapons.Count + 1, HeroEquipmentSlot.Weapon);
                    weapon.EquipmentModifiers.Add(new FlatModifier(AttributeType.DamageLow, float.Parse(data[6]), false));
                    weapon.EquipmentModifiers.Add(new FlatModifier(AttributeType.DamageHigh, float.Parse(data[7]), false));
                    weapon.EquipmentModifiers.Add(new FlatModifier(AttributeType.CritChance, float.Parse(data[9]) / 100, false));
                    weapon.EquipmentModifiers.Add(new FlatModifier(AttributeType.SpeedRating, float.Parse(data[11]), false));
                    Weapons.Add(weapon);
                    break;
                case "armour:":
                    Equipment armor = new Equipment(data[2], Armors.Count + 1, HeroEquipmentSlot.Armor);
                    armor.EquipmentModifiers.Add(new FlatModifier(AttributeType.DefenseRating, float.Parse(data[4]) / 100, false));
                    armor.EquipmentModifiers.Add(new FlatModifier(AttributeType.HitPoints, float.Parse(data[8]), true));
                    Armors.Add(armor);
                    break;
                case "combat_skill:":
                    List<string> combatData = new List<string>();
                    data = heroData[index-1].Split(new char[] { '\"' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    bool isEffectData = false;
                    foreach (var item in data)
                    {
                        if (isEffectData)
                        {
                            if (item.Trim(' ').StartsWith("."))
                                isEffectData = false;
                            else
                            {
                                combatData.Add(item);
                                continue;
                            }
                        }

                        string[] combatItems = item.Replace("%", "").Split(
                            new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (combatItems[combatItems.Length - 1] == ".effect")
                            isEffectData = true;
                        combatData.AddRange(combatItems);
                    }
                    
                    CombatSkillVariants.Add(new CombatSkill(combatData, true));
                    break;
                case "combat_move_skill:":
                    MoveSkill moveSkill = new MoveSkill();
                    moveSkill.Id = data[2];
                    moveSkill.Type = data[6];
                    moveSkill.MoveBackward = int.Parse(data[8]);
                    moveSkill.MoveForward = int.Parse(data[9]);
                    MoveSkill = moveSkill;
                    break;
                case "riposte_skill:":
                    List<string> riposteData = new List<string>();
                    data = heroData[index-1].Split(new char[] { '\"' }).ToList();
                    bool isReposteEffect = false;
                    foreach (var item in data)
                    {
                        if (isReposteEffect)
                        {
                            if (item.Trim(' ')[0] == '.')
                                isEffectData = false;
                            else
                            {
                                riposteData.Add(item);
                                continue;
                            }
                        }

                        string[] combatItems = item.Replace("%", "").Split(
                            new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (combatItems[combatItems.Length - 1] == ".effect")
                            isEffectData = true;
                        riposteData.AddRange(combatItems);
                    }
                    RiposteSkill = new CombatSkill(riposteData, true);
                    break;
                case "incompatible_party_member:":
                    IncompatiablePartyTag = data[4];
                    break;
                case "tag:":
                    Tags.Add(data[2]);
                    break;
                case "controlled:":
                    TargetRank = int.Parse(data[2]);
                    break;
                case "id_index:":
                    IndexId = int.Parse(data[2]);
                    break;
                case "skill_selection:":
                    CanSelectCombatSkills = bool.Parse(data[2]);
                    NumberOfSelectedCombatSkills = int.Parse(data[4]);
                    break;
                #region Death Door
                case "deaths_door:":
                    if (DeathDoor == null)
                        DeathDoor = new DeathDoor(data);
                    else
                        DeathDoor.LoadData(data);
                    break;
                #endregion
                #region Generation
                case "generation:":
                    if (Generation == null)
                        Generation = new HeroGeneration(data);
                    else
                        Generation.LoadData(data);
                    break;
                #endregion
                #region Battle Loot
                case "extra_battle_loot:":
                    if (ExtraBattleLoot == null)
                        ExtraBattleLoot = new LootDefinition(data);
                    else
                        ExtraBattleLoot.LoadData(data);
                    break;
                #endregion
                #region Curio Loot
                case "extra_curio_loot:":
                    if (ExtraCurioLoot == null)
                        ExtraCurioLoot = new LootDefinition(data);
                    else
                        ExtraCurioLoot.LoadData(data);
                    break;
                #endregion
                #region Inventory Stack
                case "extra_stack_limit:":
                    ExtraStackLimit = data[2];
                    break;
                #endregion
                #region Mode
                case "mode:":
                    Modes.Add(new CharacterMode(data));
                    break;
                #endregion
                default:
                    Debug.LogError("Unknown info token " + data[0] + " in hero: " + StringId);
                    break;
            }
        }

        CombatSkills = new List<CombatSkill>(CombatSkillVariants.FindAll(skill => skill.Level == 0));
    }
}