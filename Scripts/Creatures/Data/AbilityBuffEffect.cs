using Game.Creatures.Domain;
using Godot;
using System;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public partial class AbilityBuffEffect : AbilityEffect
{
    [Export] private StatEnum _stat;
    [Export(PropertyHint.Range, "1,6")] private int _intensity;

    public override void ApplyEffect(Creature playerCreature, Creature enemyCreature)
    {
        playerCreature.ChangeStatModifier(_stat, _intensity);
        GD.Print($"{playerCreature.Data.Name}'s {_stat} has been increased by {_intensity} stage(s)!");
        GD.Print($"{playerCreature.Data.Name}'s {_stat} is now at {playerCreature.GetBattleStat(_stat)}.");
    }
}