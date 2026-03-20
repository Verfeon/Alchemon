using Game.Creatures.Domain;
using Godot;
using System;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public abstract partial class AbilityEffect : Resource
{
    public abstract void ApplyEffect(Creature playerCreature, Creature enemyCreature);
}