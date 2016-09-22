﻿using UnityEngine;
using System.Collections.Generic;

public class HeroSpriteDatabase
{
    public Dictionary<string, HeroSpriteInfo> HeroClassInfo { get; set; }

    public HeroSpriteDatabase()
    {
        HeroClassInfo = new Dictionary<string, HeroSpriteInfo>();
    }

    public HeroSpriteInfo this[string classId]
    {
        get
        {
            return HeroClassInfo[classId];
        }
    }

    public Sprite GetCombatSkillIcon(Hero hero, CombatSkill combatSkill)
    {
        var spriteId = hero.HeroClass.SkillArtInfo.Find(art => art.SkillId == combatSkill.Id).IconId;
        if (spriteId != null)
            return HeroClassInfo[hero.ClassStringId].Skills[spriteId];
        else
            return null;
    }
}

public class HeroOutfit
{
    public string OutfitId { get; set; }
    public Sprite Portrait { get; set; }
}

public class HeroSpriteInfo
{
    public Dictionary<string, HeroOutfit> Outfits { get; set; }
    public Dictionary<string, Sprite> Equip { get; set; }
    public Dictionary<string, Sprite> Skills { get; set; }
    public Sprite Header { get; set; }

    public HeroOutfit this[string outfitId]
    {
        get
        {
            return Outfits[outfitId];
        }
    }

    public HeroSpriteInfo()
    {
        Outfits = new Dictionary<string, HeroOutfit>();
        Equip = new Dictionary<string, Sprite>();
        Skills = new Dictionary<string, Sprite>();
    }
}
