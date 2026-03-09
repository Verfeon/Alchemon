using Godot;
using System;
using Game.Utils;

using CreatureData = Game.Creatures.Data.CreatureData;
using CreatureNode = Game.Creatures.Presentation.CreatureNode;

namespace Game.Creatures.Domain;

public partial class CreatureSpawner : Node2D
{
	[Export] private PackedScene _creatureScenePath;
	[Export] private Godot.Collections.Array<CreatureData> _possibleCreatures;
	
	private IRandom _random = new GodotRandomAdapter();
	
	public override void _Ready()
	{
		SpawnRandomCreature();
	} 
	
	public void SpawnRandomCreature()
	{
		if (_possibleCreatures.Count == 0)
		{
			GD.PushError("CreatureNode is marked as wild but has no possible creatures assigned.");
			return;
		}
		Creature creature = new Creature(_possibleCreatures[_random.NextInt(0, _possibleCreatures.Count - 1)], 1, new GodotRandomAdapter());
		
		CreatureNode newCreature = _creatureScenePath.Instantiate<CreatureNode>();
		newCreature.Setup(creature, true);
		
		AddChild(newCreature);
	}
}
