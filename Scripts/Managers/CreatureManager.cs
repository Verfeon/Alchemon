using Godot;
using System;
using System.Collections.Generic;

using Creature = Game.Creatures.Creature;
using CreatureStats = Game.Creatures.CreatureStats;
using CreatureType = Game.Enums.CreatureType;

namespace Game.Managers;

public partial class CreatureManager : Node
{
	private List<Creature> _party = new(); // Équipe active (max 6)
	private List<Creature> _storage = new(); // Stockage PC
	private Dictionary<string, Creature> _creatureDatabase = new();
	
	public const int MaxPartySize = 6;
	
	[Signal]
	public delegate void CreatureAddedToPartyEventHandler(string creatureId);
	
	public void AddCreature(Creature creature)
	{
		if (_party.Count < MaxPartySize)
		{
			_party.Add(creature);
			EmitSignal(SignalName.CreatureAddedToParty, creature.Data.Id);
		}
		else
		{
			_storage.Add(creature);
		}
	}
	
	public void SwapCreature(int partyIndex, int storageIndex)
	{
		if (partyIndex >= 0 && partyIndex < _party.Count && 
			storageIndex >= 0 && storageIndex < _storage.Count)
		{
			var temp = _party[partyIndex];
			_party[partyIndex] = _storage[storageIndex];
			_storage[storageIndex] = temp;
		}
	}
	
	public List<Creature> GetParty()
	{
		return new List<Creature>(_party);
	}
	
	public void HealParty()
	{
		foreach (var creature in _party)
		{
			creature.Data.BaseStats.CurrentHP = creature.Data.BaseStats.MaxHP;
		}
	}
	
	public Creature GetCreatureById(string creatureId)
	{
		if (_creatureDatabase.ContainsKey(creatureId))
		{
			return _creatureDatabase[creatureId];
		}
		
		return null;
	}
}
