﻿using System.Collections.Generic;

public class TrapVariation
{
    public int Level { get; set; }
    public List<Effect> SuccessEffects;
    public List<Effect> FailEffects;
    public float HealthPenalty;

    public TrapVariation()
    {
        SuccessEffects = new List<Effect>();
        FailEffects = new List<Effect>();
    }
}

public class Trap : Prop
{
    public List<Effect> SuccessEffects;
    public List<Effect> FailEffects;
    public float HealthPenalty;

    public Dictionary<int, TrapVariation> Variations;

    public Trap(string id)
    {
        StringId = id;
        Type = AreaType.Trap;

        SuccessEffects = new List<Effect>();
        FailEffects = new List<Effect>();
        Variations = new Dictionary<int, TrapVariation>();
    }
}

